using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MallMinder.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public ExpenseController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            var expenseTypes = _context.ExpenseTypes
                .Select(x => new { Type = x.Type, Id = x.Id })
                .ToList();
            ViewBag.ExpenseTypeList = new SelectList(expenseTypes, "Id", "Type");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(ExpenseVM model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the current user asynchronously
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser != null)
                {
                    var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();

                    var expense = new Expense
                    {
                        MallId = mallId,
                        ExpenseType = model.ExpenseTypeId ?? 0,
                        ExpenseAmount = model.ExpenseAmount,
                        ExpenseDate = model.ExpenseDate,
                        IsActive = true
                    };
                }
            }
            return View();
        }
    }
}
