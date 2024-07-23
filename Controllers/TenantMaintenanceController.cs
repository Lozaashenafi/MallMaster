using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MallMinder.Controllers
{
    public class TenantMaintenanceController : Controller // or Controller, depending on your preference
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        public TenantMaintenanceController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser != null)
            {
                ViewBag.maintenanceType = new SelectList(_context.MaintenanceTypes.ToList(), "Id", "Type");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(TenantMaintenanceRequestVM model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var mallId = _context.Users
                     .Where(u => u.Id == currentUser.Id)
                     .Select(u => u.InMall) // Assuming InMall is a property of ApplicationUser
                     .FirstOrDefault();

                    var Rent = _context.Rents.Where(r => r.MallId == mallId && r.TenantId == currentUser.Id).FirstOrDefault();
                    if (Rent != null)
                    {
                        int typeId = model.TypeId;
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
                            RentId = Rent.Id,
                            MaintenanceTypeId = typeId,
                            RequestedDate = model.Date,
                            MallId = mallId,
                        };
                        _context.Maintenances.Add(maintenance);
                        _context.SaveChanges();
                        var maintenanceStatus = new MaintenanceStatus
                        {
                            MaintenanceId = maintenance.Id,
                            StatusId = 1,
                            Date = maintenance.RequestedDate,
                            IsActive = true,
                            CreatedBy = currentUser.Id, // Ensure currentUser.Id is a string
                        };
                        _context.MaintenanceStatuss.Add(maintenanceStatus);
                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Requested";
                    }
                }
            }
            return View();
        }
    }
}
