using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MallMinder.Models;
using MallMinder.Data;
using MallMinder.ViewModels;

namespace MallMinder.Controllers
{
    public class MallManagerController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public MallManagerController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(MallManagerVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "admin"); // Assign 'admin' role to the user

                    // Link user to the mall (assuming MallId is passed from the form)
                    var mallManager = new MallManagerVM
                    {
                        UserId = user.Id,
                        MallId = model.MallId
                    };

                    _context.MallManagers.Add(mallManager);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model); // Return to the view with errors if model state is invalid
        }
    }
}
