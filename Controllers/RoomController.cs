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
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id) // Adjust this according to your application's ownership logic
                    .Select(m => m.Id)
                    .FirstOrDefault();
                ViewBag.MallId = mallId;
                // Fetch floors associated with these mall IDs


                // var model = new RoomPageViewModel
                // {
                //     Floors = floors,
                //     Room = new RoomVM() // Assuming RoomVM needs initialization
                // };

                // Pass floors to the view
                return View();
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
                // Add success message to TempData
                TempData["SuccessMessage"] = "Room added successfully.";


                return RedirectToAction("AddRoom", "Room");
            }

            // If model state is not valid, return to the view with validation errors
            return View(roomVM); // Adjust according to your project's view structure
        }
    }
}