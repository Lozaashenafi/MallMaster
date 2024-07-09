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
        [HttpPost]
        public IActionResult Index(Rent rent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set the AddedDate to current datetime
                    rent.AddedDate = DateTime.Now;

                    // Add Rent object to DbContext and save changes
                    _context.Rent.Add(rent);
                    _context.SaveChanges();

                    // Redirect to a success page or another action
                    return RedirectToAction("Index", "Tenant");
                }
                catch (Exception ex)
                {
                    // Handle exception if save fails
                    ModelState.AddModelError("", "Unable to save changes. Try again later.");
                    // Log the exception (ex) here if needed
                }
            }

            // If ModelState is not valid or save fails, return to the current view with the Rent object
            return View(rent);
        }
    }
}