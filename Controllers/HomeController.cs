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

            // Room status
            var occupiedCount = _context.Rooms.Count(r => r.Status == "Occupied");
            var freeCount = _context.Rooms.Count(r => r.Status == "Free");

            var roomStatus = new RoomStatusVM
            {
                Occupied = occupiedCount,
                Free = freeCount
            };

            ViewBag.RoomStatus = roomStatus;

            // Revenue analysis
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

            ViewBag.ExpenseByYearJson = Newtonsoft.Json.JsonConvert.SerializeObject(expenseByYear);

            // Maintenance cost
            var mallMaintenance = await _context.Maintenances
                .Include(m => m.Rent)
                .ThenInclude(r => r.AppUser)
                .Where(m => m.MallId == mallId && m.CompletedDate.HasValue)
                .ToListAsync();
            var maintenanceByYear = mallMaintenance
                .GroupBy(p => p.CompletedDate.Value.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalExpense = g.Sum(p => p.Cost ?? 0)
                })
                .Where(r => r.Year == currentYear || r.Year == oneYearBefore || r.Year == twoYearsBefore || r.Year == threeYearsBefore)
                .ToDictionary(r => r.Year, r => r.TotalExpense);

            ViewBag.MaintenanceByYearJson = Newtonsoft.Json.JsonConvert.SerializeObject(maintenanceByYear);
            // Maintenance costs and tenant names
            var maintenanceCostsAndTenants = mallMaintenance
                .Where(m => m.Rent != null && m.Rent.AppUser != null)
                .GroupBy(m => m.Rent.AppUser.UserName)
                .Select(g => new
                {
                    TenantName = g.Key,
                    TotalCost = g.Sum(m => m.Cost ?? 0)
                })
                .ToList();
            ViewBag.MaintenanceCostsAndTenantsJson = Newtonsoft.Json.JsonConvert.SerializeObject(maintenanceCostsAndTenants);
            // Profit calculation
            var profitByYear = new Dictionary<int, double>();

            foreach (var year in new[] { currentYear, oneYearBefore, twoYearsBefore, threeYearsBefore })
            {
                var revenue = revenueByYear.ContainsKey(year) ? revenueByYear[year] : 0;
                var expense = expenseByYear.ContainsKey(year) ? expenseByYear[year] : 0;
                var maintenance = maintenanceByYear.ContainsKey(year) ? maintenanceByYear[year] : 0;

                var profit = revenue - (expense + maintenance);
                profitByYear[year] = profit;
            }

            ViewBag.ProfitByYearJson = Newtonsoft.Json.JsonConvert.SerializeObject(profitByYear);
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
