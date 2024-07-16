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
            var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId);

            if (manager == null)
            {
                return NotFound(); // Handle if user does not own any mall
            }

            int mallId = manager.MallId;
            ViewBag.MallId = mallId;

            return View();
        }
        [HttpPost]
        public IActionResult AddFloorPrice(PriceVM pricing)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                var mall = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId);
                if (mall == null)
                {
                    return NotFound(); // Handle if user does not own any mall
                }

                int mallId = mall.Id;
                ViewBag.MallId = mallId;
                // Create PricePerCare object

                if (pricing.PricePerCare != 0)
                {
                    // Retrieve existing PricePerCare records with the same FloorId
                    var existingPricePerCare = _context.PricePerCare.Where(x => x.FloorId == pricing.FloorNumber && x.IsActive == true).FirstOrDefault();

                    if (existingPricePerCare != null)
                    {
                        existingPricePerCare.IsActive = false;
                    }
                    // Add new PricePerCare record
                    var pricePerCare = new PricePerCare
                    {
                        FloorId = pricing.FloorNumber,
                        Price = pricing.PricePerCare,
                        MallId = mallId,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                    };

                    _context.PricePerCare.Add(pricePerCare);
                    _context.SaveChanges();
                }
                if (pricing.RoomNumber != 0 && pricing.RoomPrice != 0)
                {
                    int roomId = _context.Room.Where(r => r.Floor.MallId == mallId && r.RoomNumber == pricing.RoomNumber).Select(r => r.Id).FirstOrDefault();
                    var ofpricePerCare = _context.Room.Where(x => x.Id == roomId).FirstOrDefault();
                    if (ofpricePerCare != null)
                    {
                        ofpricePerCare.PricePercareFlag = false;
                    }
                    var roomPrice = new RoomPrice
                    {
                        RoomId = roomId,
                        Price = pricing.RoomPrice,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                    };
                    _context.RoomPrice.Add(roomPrice);
                    _context.SaveChanges();

                }

                // Redirect to a success action or view
                return RedirectToAction("Index", "Home"); // Redirect to home page or another appropriate action
            }

            // If ModelState is not valid, return to the current view with the FloorPriceVM objec
            return View();
        }
        public IActionResult AddRoom()
        {
            var userId = _userManager.GetUserId(User);

            var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId);
            if (manager == null)
            {
                return NotFound(); // Handle if user does not own any mall
            }

            int mallId = manager.MallId;
            ViewBag.MallId = mallId;


            return View();
        }
        [HttpPost]
        public IActionResult AddRoom(RoomVM roomVM)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
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
                // Get PricePerCare for the given FloorId
                var pricePerCareFloor = _context.PricePerCare
                    .Where(p => p.FloorId == roomVM.FloorId)
                    .Select(p => p.Price)
                    .FirstOrDefault();
                // Get PricePerCare for FloorId == 0 (assuming this is what you intended)
                var pricePerCareMall = _context.PricePerCare
                    .Where(p => p.FloorId == null)
                    .Select(p => p.Price)
                    .FirstOrDefault();
                float roomPrice;
                if (pricePerCareFloor != null)
                {
                    roomPrice = roomVM.Care * pricePerCareFloor;
                }
                else
                {
                    roomPrice = roomVM.Care * pricePerCareMall;
                }
                var roomPrice1 = new RoomPrice
                {
                    RoomId = room.Id,
                    Price = roomPrice,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                };
                _context.RoomPrice.Add(roomPrice1);
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