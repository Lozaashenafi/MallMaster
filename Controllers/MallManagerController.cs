using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MallMinder.Models;
using MallMinder.Data;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace MallMinder.Controllers;
[Authorize(Roles = "SystemAdmin")]


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
        var malls = _context.Malls.ToList(); // Assuming _context is your database context and Malls is your DbSet<Mall>
        ViewBag.Malls = malls.Select(mall => new SelectListItem
        {
            Value = mall.Id.ToString(),
            Text = mall.Name
        }).ToList();

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Index(MallManagerVM model)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive,
                AddedDate = model.AddedDate,
                UserName = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "admin"); // Assign 'admin' role to the user

                // Link user to the mall (assuming MallId is passed from the form)
                var mallManager = new MallManagers
                {
                    OwnerId = user.Id,
                    MallId = model.MallId
                };

                _context.MallManagers.Add(mallManager);
                await _context.SaveChangesAsync();
                // Add success message to TempData
                TempData["SuccessMessage"] = "Manager added successfully.";

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

