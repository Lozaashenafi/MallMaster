using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MallMinder.Data;
using MallMinder.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Models.ViewModels;

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
            // Fetch all Rent records along with related entities
            var rents = await _context.Rent.ToListAsync();




            // Prepare a list of TenantVM to store the data to be displayed in the view
            List<TenantVM> tenantVMs = new List<TenantVM>();

            foreach (var rent in rents)
            {
                // Fetch Tenant (AppUser) using TenantId
                var tenant = await _userManager.FindByIdAsync(rent.TenantId);


                // Fetch Room Number using RoomId
                var roomNumber = await _context.Room
                    .Where(r => r.Id == rent.RoomId)
                    .Select(r => r.RoomNumber)
                    .FirstOrDefaultAsync();

                var type = await _context.RentType.Where(r => r.Id == rent.TypeId).FirstOrDefaultAsync();
                // Fetch Floor Id using RoomId
                var floorId = await _context.Room
                    .Where(r => r.Id == rent.RoomId)
                    .Select(r => r.FloorId)
                    .FirstOrDefaultAsync();

                // Fetch Floor using FloorId
                var floor = await _context.Floor
                    .FirstOrDefaultAsync(f => f.Id == floorId);

                // Assuming Floor has a FloorNumber property
                var floorNumber = floor != null ? floor.FloorNumber : null;

                if (tenant != null)
                {
                    // Map data to TenantVM
                    var tenantVM = new TenantVM
                    {
                        TenantFirstName = tenant.FirstName, // Assuming AppUser (Tenant) has a FirstName 
                        RoomNumber = roomNumber, // Assuming Room has a RoomNumber property
                        FloorNumber = floorNumber,
                        TenantPhone = tenant.PhoneNumber,
                        RentType = type.Type
                    };

                    tenantVMs.Add(tenantVM);
                }
            }

            return View(tenantVMs);
        }

        public IActionResult AddTenant()
        {
            return View();
        }
    }
}
