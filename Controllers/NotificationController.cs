using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Data;
using MallMinder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers
{
    public class NotificationController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        public NotificationController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                var maintenance = _context.MaintenanceStatuss
                    .Include(ms => ms.Maintenance)
                        .ThenInclude(m => m.MaintenanceType)
                    .Include(ms => ms.Maintenance)
                        .ThenInclude(m => m.Rent)
                            .ThenInclude(r => r.AppUser)
                    .Include(ms => ms.Maintenance)
                        .ThenInclude(m => m.Rent)
                            .ThenInclude(r => r.Room)
                    .Where(ms => ms.StatusId == 1 && ms.Maintenance.MallId == mallId)
                    .Select(ms => new
                    {
                        Id = ms.Maintenance.Id,
                        tenantName = ms.Maintenance.Rent.AppUser.FirstName + " - Room " + ms.Maintenance.Rent.Room.RoomNumber,
                        maintenanceType = ms.Maintenance.MaintenanceType.Type
                    })
                    .ToList();
                ViewBag.maintenancerequest = maintenance;
            }
            return View();
        }
        public IActionResult Approved(int id)
        {
            // Implement your logic here
            // id parameter will have the value passed from the view
            return View();
        }

        public IActionResult Declined(int id)
        {
            // Implement your logic here
            // id parameter will have the value passed from the view
            return View();
        }
    }
}