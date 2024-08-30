
using MallMaster.Models.ViewModels;
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MallMinder.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;
        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ILogger<AccountController> logger, AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else if (roles.Contains("SystemAdmin"))
                        {
                            return RedirectToAction("Index", "System");
                        }
                        else if (roles.Contains("Tenant"))
                        {
                            return RedirectToAction("Index", "TenantDashbord");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt");
                        return RedirectToAction("Login", "Account");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View("Login", model);
                }
            }

            // If ModelState is not valid, return the login view with validation errors
            return View("Login", model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            // Check if the email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "The email address is already in use.";
                return View();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var mallId = _context.MallManagers
           .Where(m => m.OwnerId == currentUser.Id)
           .Select(m => m.MallId)
           .FirstOrDefault();
            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive,
                AddedDate = model.AddedDate,
                UserName = model.Email,
                InMall = mallId,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign "Tenant" role to the user
                await _userManager.AddToRoleAsync(user, "Tenant");

                return RedirectToAction("Index", "Tenant");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogError($"Error creating user: {error.Description}");
                }
            }


            // If ModelState is not valid or registration fails, return to the registration view with errors
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Redirect to sign-in page
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    // Sign out the user after changing the password to force re-login
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                else
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If ModelState is not valid or password change fails, return to the change password view with errors
            return View(model);
        }
        public async Task<IActionResult> Profile()
        {

            var user = await _userManager.GetUserAsync(User);
            var model = new ProfileVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ImageUrl = user.ImageUrl ?? "https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-profiles/avatar-2.webp", // Fallback image URL
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                MallName = user.Mall?.Name // Assuming Mall has a Name property
            };
            return View(model);
        }
        public async Task<IActionResult> ChangeProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // Handle the error, maybe redirect to an error page or show a message
                return NotFound();
            }

            var model = new ProfileVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ImageUrl = user.ImageUrl ?? "https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-profiles/avatar-2.webp", // Fallback image URL
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                MallName = user.Mall?.Name // Assuming Mall has a Name property
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeProfile(ProfileVM model, IFormFile ProfileImage)
        {
            if (!ModelState.IsValid)
            {
                return View("ChangeProfile", model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Handle image upload and update user.ImageUrl
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }
                user.ImageUrl = "/uploads/" + ProfileImage.FileName;
            }

            // Update user in the database
            await _userManager.UpdateAsync(user);

            // Optionally redirect or return to profile page
            return RedirectToAction("Profile");
        }


    }
}
