using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]

public class RoomController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    public RoomController(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<IActionResult> Price()
    {
        var userId = _userManager.GetUserId(User);
        var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId && m.IsActive);

        if (manager == null)
        {
            return NotFound(); // Handle if user does not own any mall
        }

        int mallId = manager.MallId;
        ViewBag.MallId = mallId;
        var pricePerCareMall = await _context.PricePerCares
                .Where(r => r.MallId == mallId && r.FloorId == null && r.IsActive == true)
                .FirstOrDefaultAsync(); // Use FirstOrDefaultAsync to get a single entity
        if (pricePerCareMall != null)
        {
            // Access the Price property from the PricePerCares entity
            double? currentPrice = pricePerCareMall.Price;
            ViewBag.currentPrice = currentPrice;
        }
        else
        {
            double? currentPrice = 0; // Or another appropriate default value
            ViewBag.currentPrice = currentPrice;
        }
        return View();
    }

    public IActionResult RoomList()
    {
        var userId = _userManager.GetUserId(User);
        var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId && m.IsActive == true);

        if (manager == null)
        {
            return NotFound("Mall Manager not found or inactive."); // Provide a more descriptive message
        }

        // Ensure mallId is properly assigned
        int mallId = manager.MallId;
        ViewBag.MallId = mallId;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Price(PriceVM pricing)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);

            var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId && m.IsActive);
            if (manager == null)
            {
                return NotFound(); // Handle if user does not own any mall
            }
            int mallId = manager.MallId;
            ViewBag.MallId = mallId;

            // Create PricePerCare object

            if (pricing.PricePerCare != 0 && pricing.PricePerCare != null)
            {
                if (pricing.FloorNumber == 0)
                {
                    pricing.FloorNumber = null;
                }
                // Retrieve existing PricePerCare records with the same FloorId
                var existingPricePerCare = _context.PricePerCares.Where(x => x.FloorId == pricing.FloorNumber && x.IsActive == true).FirstOrDefault();

                if (existingPricePerCare != null)
                {
                    existingPricePerCare.IsActive = false;
                    _context.Update(existingPricePerCare);
                    _context.SaveChanges();
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

                _context.PricePerCares.Add(pricePerCare);
                _context.SaveChanges();
            }

            if (pricing.RoomNumber != 0 && pricing.RoomPrice != 0)
            {
                int roomId = _context.Rooms.Where(r => r.Floor.MallId == mallId && r.RoomNumber == pricing.RoomNumber && r.IsActive == true).Select(r => r.Id).FirstOrDefault();
                if (roomId == null || roomId == 0)
                {
                    TempData["SuccessMessage"] = "A room doesn't exists";
                    return RedirectToAction("Index", "Room");
                }
                var ofpricePerCare = _context.Rooms.Where(x => x.Id == roomId && x.PricePercareFlag == true).FirstOrDefault();
                if (ofpricePerCare != null)
                {
                    ofpricePerCare.PricePercareFlag = false;
                }
                else
                {
                    var roomPriceold = _context.RoomPrices
                        .FirstOrDefault(r => r.Id == roomId);

                    if (roomPriceold != null)
                    {
                        // Set the IsActive property to false
                        roomPriceold.IsActive = false;
                        // Save changes to the database
                        _context.SaveChanges();
                    }
                }
                var roomPrice = new RoomPrice
                {
                    RoomId = roomId,
                    Price = pricing.RoomPrice,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                };
                _context.RoomPrices.Add(roomPrice);
                _context.SaveChanges();
            }
            TempData["SuccessMessage"] = "Successful";
            // Redirect to a success action or view
            return RedirectToAction("Price", "Room"); // Redirect to home page or another appropriate action
        }
        // If ModelState is not valid, return to the current view with the FloorPriceVM objec
        return View();
    }
    public IActionResult AddRoom()
    {
        var userId = _userManager.GetUserId(User);

        var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId && m.IsActive);
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
            var manager = _context.MallManagers.FirstOrDefault(m => m.OwnerId == userId && m.IsActive);
            int mallId = manager.MallId;
            var Exist = _context.Rooms.Include(r => r.Floor).Where(r => r.RoomNumber == roomVM.RoomNumber && r.FloorId == roomVM.FloorId && r.Floor.MallId == mallId).Any();
            if (Exist == true)
            {
                TempData["SuccessMessage"] = "A room with the specified room number already exists";
                return RedirectToAction("AddRoom", "Room");
            }
            var room = new Room
            {
                FloorId = roomVM.FloorId,
                Care = roomVM.Care,
                RoomNumber = roomVM.RoomNumber,
                Description = roomVM.Description,
                Status = "free",
                IsActive = true,
                PricePercareFlag = true,
                RoomDeactivateFlag = false,
                AddedDate = DateTime.Now
            };
            // Add room to DbContext and save changes
            _context.Rooms.Add(room);
            _context.SaveChanges();
            // Add success message to TempData
            TempData["SuccessMessage"] = "Room added successfully.";
            return RedirectToAction("AddRoom", "Room");
        }
        // If model state is not valid, return to the view with validation errors
        return View(roomVM); // Adjust according to your project's view structure
    }

    public async Task<IActionResult> EditRoom(Room room)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            // Redirect to a login page or show an error message
            return RedirectToAction("Login", "Account");
        }

        var existingRoom = await _context.Rooms.FindAsync(room.Id);
        if (existingRoom == null)
        {
            // Handle the case where the room was not found
            return NotFound();
        }

        // Update the existing room properties with the new values
        existingRoom.FloorId = room.FloorId;
        existingRoom.Care = room.Care;
        existingRoom.Description = room.Description;
        // Add any other properties that need to be updated

        try
        {
            await _context.SaveChangesAsync();
            // Optionally add a success message
            TempData["SuccessMessage"] = "Room details updated successfully.";
            return RedirectToAction("RoomList"); // Redirect to the appropriate page
        }
        catch (Exception ex)
        {
            // Log the exception and handle it
            ModelState.AddModelError(string.Empty, "An error occurred while updating the room. Please try again.");
        }
        // If we get here, something went wrong, redisplay the form
        return View(room);
    }
    [HttpPost]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            TempData["SuccessMessage"] = "Room not found.";
            return RedirectToAction("Index");
        }
        // Trim any whitespace from room.Status
        var status = room.Status.Trim();
        if (status == "Occupied")
        {
            TempData["SuccessMessage"] = "Room is Occupied and cannot be deleted.";
            return RedirectToAction("Index");
        }

        // Deactivate the room or remove it based on your requirement
        room.IsActive = false;

        // Alternatively, if you want to remove the room from the context
        //_context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Room deleted successfully.";
        return RedirectToAction("RoomList");
    }

}
