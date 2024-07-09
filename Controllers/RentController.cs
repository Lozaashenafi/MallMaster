using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                // Find mall IDs owned by the current user
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id) // Adjust this according to your application's ownership logic
                    .Select(m => m.Id)
                    .FirstOrDefault();
                ViewBag.MallId = mallId;
                var rooms = _context.Room
                .Where(r => _context.Floor.Any(f => f.Id == r.FloorId && f.MallId == mallId))
                .Where(r => r.Status == "Free");
                // ViewBag.rooms = rooms;
                // Fetch floors associated with these mall IDs
                return View();
            }

            // Handle case where user is not found or has no associated malls
            return NotFound();
        }
    }
}