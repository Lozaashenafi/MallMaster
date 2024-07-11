using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Data;
using MallMinder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                var rooms = _context.Room
                    .Where(r => _context.Floor.Any(f => f.Id == r.FloorId && f.MallId == mallId))
                    .Where(r => r.Status == "Occupied")
                    .ToList(); // Materialize the rooms query
                var roomIds = rooms.Select(r => r.Id).ToList(); // Materialize room IDs
                var rents = _context.Rent
                    .Where(rent => roomIds.Contains(rent.RoomId))
                    .ToList();
                var maintenanceStatusType =
                ViewBag.maintenanceType = new SelectList(_context.MaintenanceType.ToList(), "Id", "Type");
                ViewBag.rents = rents;
            }
            return View();
        }

    }
}