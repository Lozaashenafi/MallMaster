
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                    .Where(m => m.OwnerId == currentUser.Id)
                    .Select(m => m.Id)
                    .FirstOrDefault();

                // Step 1: Fetch Occupied Rooms in the Current Mall
                var occupiedRooms = _context.Room
                    .Where(r => _context.Floor.Any(f => f.Id == r.FloorId && f.MallId == mallId))
                    .Where(r => r.Status == "Occupied")
                    .ToList();

                // Step 2: Fetch Room IDs
                var roomIds = occupiedRooms.Select(r => r.Id).ToList();
                // Step 3: Fetch Rent Details including Tenant Information
                var rents = _context.Rent
                .Include(r => r.Room)     // Include room details
                .Where(r => roomIds.Contains(r.RoomId))
                .Select(r => new
                {
                    RentId = r.Id,
                    RoomNumber = "Room  - " + " " + r.Room.RoomNumber,
                })
                .ToList();
                // 'rents' now contains a list of objects with RentId, RoomNumber, and TenantName

                ViewBag.maintenanceType = new SelectList(_context.MaintenanceType.ToList(), "Id", "Type");
                ViewBag.rents = new SelectList(rents, "RentId", "RoomNumber");

            }
            return View();
        }
        [HttpPost]
        // public async Task<IActionResult> Index(MaintenanceRequestVM model)
        // {
        //     if (ModelState.IsValid)
        //     {

        //         var maintenance = new Maintenance
        //         {
        //             RentId = model.RentId,
        //             MaintenanceTypeId = model.MaintenanceTypeId,
        //             RequestedDate = model.RequestedDate,
        //         };

        //     }

        // }


    }
}