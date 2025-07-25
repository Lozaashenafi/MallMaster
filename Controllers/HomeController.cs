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
            var occupiedCount = _context.Rooms.Include(r => r.Floor).Where(r => r.Floor.MallId == mallId).Count(r => r.Status == "Occupied");
            var freeCount = _context.Rooms.Include(r => r.Floor).Where(r => r.Floor.MallId == mallId).Count(r => r.Status == "Free");

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
            // Get the current year
            // Filter, group by tenant, and order by total cost in descending order, then take the top 4
            var top4MaintenanceCostsAndTenants = mallMaintenance
                .Where(m => m.Rent != null && m.Rent.AppUser != null && m.CompletedDate.Value.Year == currentYear)
                .GroupBy(m => m.Rent.AppUser.FirstName + " " + m.Rent.AppUser.LastName)
                .Select(g => new
                {
                    TenantName = g.Key,
                    TotalCost = g.Sum(m => m.Cost ?? 0)
                })
                .OrderByDescending(g => g.TotalCost)
                .Take(4)
                .ToList();
            ViewBag.MaintenanceCostsAndTenantsJson = Newtonsoft.Json.JsonConvert.SerializeObject(top4MaintenanceCostsAndTenants);
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
            // Revenue analysis
            var paymentsThisYear = await _context.TenantPayments
                .Include(p => p.Rent)
                .ThenInclude(r => r.Mall)
                .Where(p => p.Rent.MallId == mallId && p.PaidDate.Year == currentYear)
                .ToListAsync();

            double totalRevenue = paymentsThisYear.Sum(p => p.Price ?? 0);
            ViewBag.TotalRevenue = totalRevenue;

            // Expense analysis
            var thisMallExpensesThisYear = await _context.Expenses
                .Include(x => x.Mall)
                .Where(x => x.MallId == mallId && x.ExpenseDate.Value.Year == currentYear)
                .ToListAsync();

            double totalExpense = thisMallExpensesThisYear.Sum(p => p.ExpenseAmount ?? 0);
            ViewBag.TotalExpense = totalExpense;

            // Maintenance cost
            var mallMaintenanceThisYear = await _context.Maintenances
                .Include(m => m.Rent)
                .ThenInclude(r => r.AppUser)
                .Where(m => m.MallId == mallId && m.CompletedDate.HasValue && m.CompletedDate.Value.Year == currentYear)
                .ToListAsync();
            double totalMaintenance = mallMaintenanceThisYear.Sum(p => p.Cost ?? 0);
            ViewBag.TotalMaintenance = totalMaintenance;

            var currentMonth = DateTime.Now.Month;

            // Sum of revenue for the current month
            var revenueForCurrentMonth = payments
                .Where(p => p.PaidDate.Year == currentYear && p.PaidDate.Month == currentMonth)
                .Sum(p => p.Price ?? 0);
            ViewBag.revenueForCurrentMonth = revenueForCurrentMonth;
            // Profit calculation
            double profitThisYear = totalRevenue - (totalExpense + totalMaintenance);
            ViewBag.TotalProfit = profitThisYear;
        };

        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

}
