using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace MallMinder.Controllers
{
    public class RentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public RentController(UserManager<AppUser> userManager, AppDbContext context)
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
                    .Where(m => m.OwnerId == currentUser.Id) // Adjust this according to your application's ownership logic
                    .Select(m => m.Id)
                    .FirstOrDefault();

                var rooms = _context.Room.Include(x => x.Floor).Where(r => r.Floor.MallId == mallId && r.Status == "free").Select(r => new
                {
                    Id = r.Id,
                    textVal = r.RoomNumber + "-" + r.Floor.FloorNumber
                }).ToList();

                ViewBag.floors = new SelectList(_context.Floor.Where(f => f.MallId == mallId).ToList(), "Id", "FloorNumber");
                ViewBag.Rooms = new SelectList(rooms, "Id", "textVal");
                ViewBag.rentTypes = new SelectList(_context.RentType.ToList(), "Id", "Type");
                var model = new RentVM();

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

                // Fetch TenantId from AppUser based on TenantUserName
                var tenantUser = _context.Users.FirstOrDefault(u => u.UserName == rentVM.TenantUserName);
                if (tenantUser == null)
                {
                    ModelState.AddModelError("", "Tenant user not found.");
                    return View(rentVM);
                }

                // Fetch room
                var room = _context.Room.FirstOrDefault(r => r.Id == rentVM.RoomId);
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

                    _context.RentType.Add(otherType);
                    _context.SaveChanges();
                    typeId = otherType.Id;
                }

                // Create Rent object
                var rent = new Rent
                {
                    RoomId = rentVM.RoomId,
                    TenantId = tenantUser.Id,
                    RentalDate = rentVM.RentalDate,
                    PaymentDuration = rentVM.PaymentDuration, // Default to 0 if PaymentDuration is null
                    AddedDate = DateTime.Now,
                    TypeId = typeId, // Assign RentType Id
                    CreatedBy = currentUser.Id
                };

                // Add Rent object to DbContext and save changes
                _context.Rent.Add(rent);
                _context.SaveChanges();

                // Update Room status to 'Occupied'
                room.Status = "Occupied";
                _context.SaveChanges();
                return RedirectToAction("Index", "Tenant");
            }

            // If ModelState is not valid or save fails, return to the current view with the RentVM object
            return View(rentVM);
        }


    }
}
