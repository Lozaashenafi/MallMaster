using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels; // Make sure to include your view model namespace
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MallMinder.Controllers
{
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

        public async Task<IActionResult> Index()
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
                var tenantUsers = users.Where(u => _userManager.IsInRoleAsync(u, "Tenant").Result && u.InMall == mallId).ToList();

                // Map to TenantVM
                var tenantVMs = tenantUsers.Select(u => new TenantVM
                {
                    Id = u.Id,
                    TenantFirstName = u.FirstName,
                    TenantPhone = u.PhoneNumber
                }).ToList();

                // Return the view with the TenantVM list
                return View(tenantVMs);
            }
            catch (Exception ex)
            {
                // Handle exception as per your application's error handling strategy
                // Log the exception or redirect to an error page
                return RedirectToAction("Index", "Home"); // Example redirect to home page
            }
        }

        public IActionResult AddTenant()
        {
            return View();
        }
    }
}
