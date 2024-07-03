using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MallMinder.Data;
using MallMinder.Models;
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
            // Ensure the role 'Tenant' exists
            var tenantRole = await _roleManager.FindByNameAsync("Tenant");
            if (tenantRole == null)
            {
                // Handle role not found
                return NotFound();
            }

            // Fetch all users
            var users = await _userManager.Users.ToListAsync();

            // Filter users who are in the 'Tenant' role
            var usersInRole = users.Where(u => _userManager.IsInRoleAsync(u, "Tenant").Result).ToList();

            return View(usersInRole);
        }

        public IActionResult AddTenant()
        {
            return View();
        }
    }
}
