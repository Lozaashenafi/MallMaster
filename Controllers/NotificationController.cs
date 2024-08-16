using System.Linq;
using MallMinder.Data;
using MallMinder.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]

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
                .Where(ms => ms.StatusId == 1 && ms.IsActive == true && ms.Maintenance.MallId == mallId)
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
    public async Task<IActionResult> Approved(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var maintenance = _context.MaintenanceStatuss
            .Where(r => r.MaintenanceId == id && r.IsActive == true)
            .FirstOrDefault();
        int? approvedId = _context.MaintenanceStatusTypes
                    .Where(r => r.SysCode == 2)
                    .Select(r => r.Id)
                    .SingleOrDefault();
        if (maintenance != null)
        {
            var newStatus = new MaintenanceStatus();
            newStatus.MaintenanceId = maintenance.MaintenanceId;
            newStatus.Date = DateTime.Now;
            newStatus.CreatedBy = currentUser.Id;
            newStatus.StatusId = approvedId;
            newStatus.IsActive = true;
            _context.MaintenanceStatuss.Add(newStatus);
            _context.SaveChanges(); // Save changes asynchronously

            maintenance.IsActive = false;
            _context.Update(maintenance);
            _context.SaveChanges();
        }
        return RedirectToAction("Index", "Notification");
    }
    public async Task<IActionResult> Declined(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var maintenance = _context.MaintenanceStatuss
            .FirstOrDefault(r => r.MaintenanceId == id && r.IsActive == true);
        if (maintenance != null)
        {
            int? declineId = _context.MaintenanceStatusTypes
                        .Where(r => r.SysCode == 3)
                        .Select(r => r.Id)
                        .SingleOrDefault();
            var maintenance1 = new MaintenanceStatus
            {
                MaintenanceId = maintenance.MaintenanceId,
                StatusId = declineId,
                Date = DateTime.Now,
                CreatedBy = currentUser.Id,
                IsActive = true,
            };
            _context.MaintenanceStatuss.Add(maintenance1);
            _context.SaveChanges(); // Save changes asynchronously
            maintenance.IsActive = false;
            _context.Update(maintenance);
            _context.SaveChanges();
        }
        return RedirectToAction("Index", "Notification");
    }
}
