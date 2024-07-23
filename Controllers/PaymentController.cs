
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers
{
    public class PaymentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public PaymentController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id)
                .Select(m => m.MallId)
                .FirstOrDefault(); // Assuming async usage
            var users = await _userManager.Users.ToListAsync();
            var Rents = _context.Rents
                    .Include(r => r.Room)
                    .Include(r => r.AppUser)
                    .Where(r => r.MallId == mallId && r.IsActive)
                    .Select(r => new PaymentVM
                    {
                        TenantName = r.AppUser.FirstName + " " + r.AppUser.LastName,
                        TenantPhone = r.AppUser.PhoneNumber
                    })
                    .ToList();
            List<PaymentVM> Rentlist = Rents;

            // Step 1: Fetch Occupied Rooms in the Current Mall
            var Rooms = _context.Rooms
                .Where(r => _context.Floors.Any(f => f.Id == r.FloorId && f.MallId == mallId) && r.IsActive == true)
                .ToList();
            // Step 2: Fetch Room IDs
            var roomIds = Rooms.Select(r => r.Id).ToList();
            // Step 3: Fetch Rent Details including Tenant Information
            var rents = _context.Rents
            .Include(r => r.Room)     // Include room details
            .Include(r => r.AppUser)
            .Where(r => roomIds.Contains(r.RoomId) && r.IsActive == true && r.AppUser.Id == r.TenantId)
            .Select(r => new
            {
                RentId = r.Id,
                RoomNumber = r.AppUser.FirstName + "  Room  - " + " " + r.Room.RoomNumber,
            })
            .ToList();
            ViewBag.rents = new SelectList(rents, "RentId", "RoomNumber");
            return View(Rentlist);



        }
        [HttpPost]
        public async Task<IActionResult> Index(PaymentVM model)
        {
            if (ModelState.IsValid)
            {
            }
            return View();
        }
    }
}