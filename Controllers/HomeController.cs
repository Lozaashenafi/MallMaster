using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MallMinder.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MallMinder.Data;
using MallMinder.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public HomeController(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = _userManager.GetUserAsync(User).Result;

        if (currentUser != null)
        {
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                .Select(m => m.MallId)
                .FirstOrDefault();

            // for roomstatus
            var occupiedCount = _context.Rooms.Count(r => r.Status == "Occupied");
            var freeCount = _context.Rooms.Count(r => r.Status == "Free");

            var roomStatus = new RoomStatusVM
            {
                Occupied = occupiedCount,
                Free = freeCount
            };

            ViewBag.RoomStatus = roomStatus;


            // Revenue anaysis
            int currentYear = DateTime.Now.Year;
            int oneYearBefore = currentYear - 1;
            int twoYearsBefore = currentYear - 2;
            int threeYearsBefore = currentYear - 3;
            var payments = await _context.TenantPayments
                    .Include(p => p.Rent)
                    .ThenInclude(r => r.Mall)
                    .Where(p => p.Rent.MallId == mallId)
                    .ToListAsync();

            var revenueByYear = payments
        .GroupBy(p => p.PaidDate.Year)
        .Select(g => new
        {
            Year = g.Key,
            TotalRevenue = g.Sum(p => p.Price ?? 0)
        })
        .Where(r => r.Year == currentYear || r.Year == oneYearBefore || r.Year == twoYearsBefore || r.Year == threeYearsBefore)
        .ToDictionary(r => r.Year, r => r.TotalRevenue);

            // Convert the dictionary to JSON
            ViewBag.RevenueByYearJson = Newtonsoft.Json.JsonConvert.SerializeObject(revenueByYear);


            // Expense analysis
            var thisMallExpenses = await _context.Expenses
                .Include(x => x.Mall)
                .Where(x => x.MallId == mallId)
                .ToListAsync();


            var expenseByYear = thisMallExpenses
                .GroupBy(p => p.ExpenseDate.Value.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalExpense = g.Sum(p => p.ExpenseAmount ?? 0)
                })
                .Where(r => r.Year == currentYear || r.Year == oneYearBefore || r.Year == twoYearsBefore || r.Year == threeYearsBefore)
                .ToDictionary(r => r.Year, r => r.TotalExpense);

            // Convert the dictionary to JSON
            ViewBag.ExpenseByYearJson = Newtonsoft.Json.JsonConvert.SerializeObject(expenseByYear);


        };
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
