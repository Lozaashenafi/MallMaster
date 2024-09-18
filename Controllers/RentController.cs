using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]

public class RentController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public RentController(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index(string id, string name)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
        {
            return BadRequest(); // Handle invalid parameters
        }
        var currentUser = _userManager.GetUserAsync(User).Result;

        if (currentUser != null)
        {
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                .Select(m => m.MallId)
                .FirstOrDefault();

            var rooms = _context.Rooms.Include(x => x.Floor).Where(r => r.Floor.MallId == mallId && r.Status == "free" && r.IsActive == true).Select(r => new
            {
                Id = r.Id,
                textVal = r.RoomNumber + "-" + r.Floor.FloorNumber
            }).ToList();

            ViewBag.floors = new SelectList(_context.Floors.Where(f => f.MallId == mallId && f.IsActive == true).ToList(), "Id", "FloorNumber");
            ViewBag.Rooms = new SelectList(rooms, "Id", "textVal");
            ViewBag.rentTypes = new SelectList(_context.RentTypes.ToList(), "Id", "Type");
            var model = new RentVM();
            ViewBag.TenantId = id;
            ViewBag.TenantName = name;
            // Initialize other properties as needed
            return View(model);
        }

        // Handle case where user is not found or has no associated malls
        return NotFound();
    }
    [HttpPost]
    public IActionResult Index(RentVM rentVM)
    {
        if (ModelState.IsValid)
        {

            var currentUser = _userManager.GetUserAsync(User).Result;
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                .Select(m => m.MallId)
                .FirstOrDefault();

            var room = _context.Rooms.FirstOrDefault(r => r.Id == rentVM.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("", "Room not found.");
                return View(rentVM);
            }

            int typeId = rentVM.TypeId ?? 0; // Default to 0 if TypeId is null
            if (!string.IsNullOrEmpty(rentVM.Other))
            {
                var otherType = new RentType
                {
                    Tag = "Other",
                    Type = rentVM.Other
                };

                _context.RentTypes.Add(otherType);
                _context.SaveChanges();
                typeId = otherType.Id;
            }

            // Create Rent object
            var rent = new Rent
            {
                RoomId = rentVM.RoomId,
                TenantId = rentVM.TenantId,
                RentalDate = rentVM.RentalDate,
                PaymentDuration = rentVM.PaymentDuration, // Default to 0 if PaymentDuration is null
                AddedDate = DateTime.Now,
                TypeId = typeId, // Assign RentType Id
                CreatedBy = currentUser.Id,
                MallId = mallId,
            };

            // Add Rent object to DbContext and save changes
            _context.Rents.Add(rent);
            _context.SaveChanges();
            var roomOccupancy = new RoomOccupancies
            {
                RoomId = rentVM.RoomId,
                OccupiedDate = rentVM.RentalDate,

            };
            _context.RoomOccupancies.Add(roomOccupancy);
            _context.SaveChanges();
            // Update Room status to 'Occupied'
            room.Status = "Occupied";
            _context.SaveChanges();
            return RedirectToAction("TenantList", "Tenant");
        }

        // If ModelState is not valid or save fails, return to the current view with the RentVM object
        return View(rentVM);
    }
    [HttpPost]
    public async Task<IActionResult> RemoveRent(int rentId)
    {
        if (rentId <= 0)
        {
            return BadRequest("Invalid rent ID.");
        }

        // Find the rent record by rentId
        var rent = await _context.Rents
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == rentId);

        if (rent == null)
        {
            return NotFound("Rent record not found.");
        }

        // Set the rent record as inactive
        rent.IsActive = false;

        // Find the room occupancy record
        var occupancy = await _context.RoomOccupancies
            .Where(r => r.RoomId == rent.RoomId)
            .FirstOrDefaultAsync();

        // If an occupancy record is found, set the ReleasedDate
        if (occupancy != null)
        {
            occupancy.ReleasedDate = DateTime.Now;
        }

        // Update room status only if it's not already free
        if (rent.Room != null && rent.Room.Status != "free")
        {
            rent.Room.Status = "free";
        }

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Redirect or return a view indicating success
        return RedirectToAction("TenantList", "Tenant");
    }
}
