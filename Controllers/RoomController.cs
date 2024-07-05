using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MallMinder.Controllers
{
    public class RoomController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public RoomController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddRoom()
        {
            // Get the current user
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                // Find mall IDs owned by the current user
                var mallIds = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id) // Adjust this according to your application's ownership logic
                    .Select(m => m.Id)
                    .ToList();

                // Fetch floors associated with these mall IDs
                var floors = _context.Floor
                    .Where(f => mallIds.Contains(f.MallId))
                    .ToList();

                // Pass floors to the view
                return View(floors);
            }

            // Handle case where user is not found or has no associated malls
            return NotFound();
        }
        [HttpPost]
        public IActionResult AddRoom(RoomVM roomVM)
        {
            if (ModelState.IsValid)
            {
                // Map RoomVM to Room entity
                var room = new Room
                {
                    FloorId = roomVM.FloorId,
                    Care = roomVM.Care,
                    RoomNumber = roomVM.RoomNumber,
                    Description = roomVM.Description,
                    Status = roomVM.Status,
                    IsActive = true,
                    PricePercareFlag = true,
                    RoomDeactivateFlag = false,
                    AddedDate = DateTime.Now
                };

                // Add room to DbContext and save changes
                _context.Room.Add(room);
                _context.SaveChanges();

                return RedirectToAction("Index", "Room"); // Replace with appropriate redirect
            }

            // If model state is not valid, return to the view with validation errors
            return View(roomVM); // Adjust according to your project's view structure
        }
    }
}