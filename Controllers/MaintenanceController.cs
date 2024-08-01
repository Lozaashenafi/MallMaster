using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public MaintenanceController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();

                // Step 1: Fetch Occupied Rooms in the Current Mall
                var Rooms = _context.Rooms
                    .Where(r => _context.Floors.Any(f => f.Id == r.FloorId && f.MallId == mallId) && r.IsActive == true)
                    .ToList();

                // Step 2: Fetch Room IDs
                var roomIds = Rooms.Select(r => r.Id).ToList();
                // Step 3: Fetch Rent Details including Tenant Information
                var rents = _context.Rents
                .Include(r => r.Room)     // Include room details
                .Include(r => r.AppUser)
                .Where(r => roomIds.Contains(r.RoomId) && r.IsActive == true && r.AppUser.Id == r.TenantId)
                .Select(r => new
                {
                    RentId = r.Id,
                    RoomNumber = "Room  - " + " " + r.Room.RoomNumber + " - " + r.AppUser.FirstName,
                })
                .ToList();
                // 'rents' now contains a list of objects with RentId, RoomNumber, and TenantName
                ViewBag.maintenanceType = new SelectList(_context.MaintenanceTypes.ToList(), "Id", "Type");
                int? approvedId = _context.MaintenanceStatusTypes
                            .Where(r => r.SysCode == 2)
                            .Select(r => r.Id)
                            .SingleOrDefault();
                ViewBag.rents = new SelectList(rents, "RentId", "RoomNumber");
                var maintenanceData = _context.MaintenanceStatuss
                        .Include(m => m.Maintenance)
                        .ThenInclude(m => m.Rent)
                        .ThenInclude(m => m.Room)
                        .Include(m => m.Maintenance.MaintenanceType)
                        .Where(m => m.Maintenance.MallId == mallId && m.IsActive == true && m.StatusId == approvedId)
                        .Select(m => new
                        {
                            MaintenanceId = m.Maintenance.Id,
                            RoomNumber = "Room " + m.Maintenance.Rent.Room.RoomNumber + " - " + m.Maintenance.MaintenanceType.Type,
                        }).ToList();
                ViewBag.maintenanceData = new SelectList(maintenanceData, "MaintenanceId", "RoomNumber");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(MaintenanceVM model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the current user asynchronously
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                    if ((model.RentId != 0 && model.RentId != null) && (model.MaintenanceTypeId != 0 && model.MaintenanceTypeId != null))
                    {
                        int typeId = model.MaintenanceTypeId ?? 0;
                        if (!string.IsNullOrEmpty(model.Other))
                        {
                            var otherType = new MaintenanceType
                            {
                                Type = model.Other
                            };
                            _context.MaintenanceTypes.Add(otherType);
                            _context.SaveChanges();
                            typeId = otherType.Id;
                        }
                        var maintenance = new Maintenance
                        {
                            RentId = model.RentId,
                            MaintenanceTypeId = typeId,
                            RequestedDate = model.RequestedDate,
                            MallId = mallId,
                        };
                        _context.Maintenances.Add(maintenance);
                        _context.SaveChanges();
                        int? approvedId = _context.MaintenanceStatusTypes
                            .Where(r => r.SysCode == 2)
                            .Select(r => r.Id)
                            .SingleOrDefault();
                        var maintenanceStatus = new MaintenanceStatus
                        {
                            MaintenanceId = maintenance.Id,
                            StatusId = approvedId,
                            Date = maintenance.RequestedDate,
                            IsActive = true,
                            CreatedBy = currentUser.Id, // Ensure currentUser.Id is a string
                        };
                        var statusesToUpdate = _context.MaintenanceStatuss.Where(m => m.MaintenanceId == maintenance.Id);
                        // Loop through each record and set IsActive to false
                        foreach (var status in statusesToUpdate)
                        {
                            status.IsActive = false;
                        }
                        // Save changes to the database
                        _context.SaveChanges();
                        // Add new maintenance status
                        _context.MaintenanceStatuss.Add(maintenanceStatus);
                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Approved";
                    }
                    if (model.Cost != 0 && model.MaintenanceId != 0)
                    {
                        var maintenance = _context.Maintenances.FirstOrDefault(m => m.Id == model.MaintenanceId);

                        if (maintenance != null)
                        {
                            maintenance.Cost = model.Cost;
                            maintenance.CompletedDate = DateTime.Now;
                            _context.SaveChanges(); // Save changes to the database

                            var status = _context.MaintenanceStatuss.Where(m => m.MaintenanceId == maintenance.Id && m.IsActive == true).FirstOrDefault();
                            if (status != null)
                            {
                                status.IsActive = false;
                            }
                            int? doneId = _context.MaintenanceStatusTypes
                            .Where(r => r.SysCode == 4)
                            .Select(r => r.Id)
                            .SingleOrDefault();
                            var newStatus = new MaintenanceStatus
                            {
                                MaintenanceId = maintenance.Id,
                                StatusId = doneId,
                                Date = maintenance.RequestedDate,
                                IsActive = true,
                                CreatedBy = currentUser.Id,
                            };
                            _context.MaintenanceStatuss.Add(newStatus);
                            _context.SaveChanges();
                        }
                        TempData["SuccessMessage"] = "Complited";
                    }
                }
            }
            return RedirectToAction("Index", "Maintenance");
        }
    }
}