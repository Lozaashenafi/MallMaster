using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]

public class TenantController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    public TenantController(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> TenantList()
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not authenticated
            }
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id)
                .Select(m => m.MallId)
                .FirstOrDefault(); // Assuming async usage

            var users = await _userManager.Users.ToListAsync();

            // Filter users who are in the 'Tenant' role and belong to the current mall
            var tenants = users.Where(u => _userManager.IsInRoleAsync(u, "Tenant").Result && u.InMall == mallId && u.IsActive == true).ToList();


            List<TenantVM> tenantlist = new List<TenantVM>();
            foreach (var tenant in tenants)
            {
                var tenantVM = new TenantVM();
                var tenatrent = _context.Rents.Include(x => x.Room).ThenInclude(x => x.Floor).Include(x => x.RentType).Where(r => r.TenantId == tenant.Id && r.IsActive == true).FirstOrDefault();
                tenantVM.RentId = tenatrent?.Id;
                tenantVM.TenantId = tenant.Id;
                tenantVM.TenantName = tenant.FirstName + " " + tenant.LastName;
                tenantVM.TenantPhone = tenant.PhoneNumber;
                tenantVM.RoomNumber = tenatrent?.Room?.RoomNumber;
                tenantVM.FloorNumber = tenatrent?.Room?.Floor?.FloorNumber;
                tenantVM.RentType = tenatrent?.RentType?.Type;
                tenantlist.Add(tenantVM);
            };

            // Return the view with the TenantVM list
            return View(tenantlist);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Index", "Home"); // Example redirect to home page
        }
    }
    public async Task<IActionResult> TenantDetail(int id, string name)
    {
        if (id == 0 || string.IsNullOrEmpty(name))
        {
            return BadRequest();
        }

        var rent = await _context.Rents
            .Include(r => r.AppUser)
            .Include(r => r.Room)
                .ThenInclude(r => r.Floor)
            .Include(r => r.RentType)
            .SingleOrDefaultAsync(r => r.Id == id);

        if (rent == null)
        {
            return NotFound();
        }

        var rentPaymentStatus = await _context.TenantPayments
            .Where(tp => tp.RentId == rent.Id && tp.IsActive)
            .FirstOrDefaultAsync();

        string status = "never Paid";

        if (rentPaymentStatus != null)
        {
            DateTime paymentDate = rentPaymentStatus.PaymentDate;
            DateTime currentDate = DateTime.Now;
            int duration = rent.PaymentDuration;
            int durationInDays = duration * 30;

            TimeSpan difference = currentDate - paymentDate;
            double differenceInDays = difference.TotalDays;

            if (differenceInDays > durationInDays)
            {
                int overdueDays = (int)(differenceInDays - durationInDays);
                int month = (int)(overdueDays / 30);
                int day = (int)(overdueDays % 30);
                status = $"{month} month - {day}days Passed";
            }
            else
            {
                status = "Paid";
            }
        }

        var tenantDetail = new TenantDetailVM
        {
            TenantName = rent.AppUser.FirstName + " " + rent.AppUser.LastName,
            RentId = rent.Id,
            Room = rent.Room.RoomNumber,
            Floor = rent.Room.Floor.FloorNumber,
            Type = rent.RentType.Type,
            RentedDate = rent.RentalDate,
            PaymentStatus = status,
        };

        return View(tenantDetail); // Pass a single TenantDetailVM object
    }

    public IActionResult AddTenant()
    {
        return View();
    }
}

