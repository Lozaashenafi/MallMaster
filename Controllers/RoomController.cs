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
            var userId = _userManager.GetUserId(User);

            var mall = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId);
            if (mall == null)
            {
                return NotFound(); // Handle if user does not own any mall
            }

            var mallId = mall.Id;
            ViewBag.MallId = mallId;

            // Fetch all rooms data associated with the mall
            var rooms = _context.Room
                .Where(r => _context.Floor.Any(f => f.Id == r.FloorId && f.MallId == mallId))
                .ToList(); // Execute the query and convert to List<Room>

            return View(rooms);
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