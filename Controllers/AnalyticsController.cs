using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Data;
using MallMinder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public AnalyticsController(UserManager<AppUser> userManager, AppDbContext context)
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
                // maintenance 
                var now = DateTime.Now;
                var maintenanceDataOnProgress = _context.MaintenanceStatuss
                    .Include(r => r.Maintenance)
                        .ThenInclude(r => r.MaintenanceType)
                    .Include(r => r.Maintenance.Rent)
                        .ThenInclude(r => r.AppUser)
                    .Include(r => r.MaintenanceStatusType)
                    .Where(r => r.Maintenance.MallId == mallId && r.Maintenance.CompletedDate == null && r.IsActive == true)
                    .ToList() // Materialize the query here
                    .Where(r => r.MaintenanceStatusType.SysCode == 1 || r.MaintenanceStatusType.SysCode == 2) // Filter in memory
                    .Select(r => new
                    {
                        TenantName = r.Maintenance.Rent.AppUser.FirstName + " " + r.Maintenance.Rent.AppUser.LastName,
                        TenantStatus = r.MaintenanceStatusType.Type,
                        RequestedDate = r.Maintenance.RequestedDate,
                        MaintenanceType = r.Maintenance.MaintenanceType.Type,
                        DaysSinceRequested = r.Maintenance.RequestedDate.HasValue
                        ? (int)Math.Floor((now - r.Maintenance.RequestedDate.Value).TotalDays)
                        : (int?)null
                    })
                    .OrderByDescending(r => r.DaysSinceRequested)
                    .ToList();

                ViewBag.MaintenanceOnProgressJson = Newtonsoft.Json.JsonConvert.SerializeObject(maintenanceDataOnProgress);
                var twelveMonthsAgo = now.AddMonths(-12);

                var maintenanceDataComplited = _context.MaintenanceStatuss
                    .Include(r => r.Maintenance)
                        .ThenInclude(r => r.MaintenanceType)
                    .Include(r => r.Maintenance.Rent)
                        .ThenInclude(r => r.AppUser)
                    .Include(r => r.MaintenanceStatusType)
                    .Where(r => r.Maintenance.MallId == mallId && r.IsActive == true)
                    .Where(r => r.MaintenanceStatusType.SysCode == 3 || r.MaintenanceStatusType.SysCode == 4)
                    .Where(r => r.Maintenance.CompletedDate >= twelveMonthsAgo) // Filter by last 12 months
                    .Select(r => new
                    {
                        TenantName = r.Maintenance.Rent.AppUser.FirstName + " " + r.Maintenance.Rent.AppUser.LastName,
                        TenantStatus = r.MaintenanceStatusType.Type,
                        RequestedDate = r.Maintenance.RequestedDate,
                        MaintenanceType = r.Maintenance.MaintenanceType.Type,
                        CompletedDate = r.Maintenance.CompletedDate // Ensure property name is correct
                    })
                    .OrderByDescending(r => r.CompletedDate) // Use the correct property name
                    .ToList();

                ViewBag.MaintenanceDataComplitedJson = Newtonsoft.Json.JsonConvert.SerializeObject(maintenanceDataComplited);
                // expense
                var ExpenseProgress = _context.Expenses.Include(e => e.ExpenseType).Where(e => e.MallId == mallId && e.IsAcrive == true).Where(e => e.ExpenseDate >= twelveMonthsAgo).Select(e => new
                {
                    ExpenseType = e.ExpenseType.Type,
                    Description = e.Description,
                    ExceptionDate = e.ExpenseDate,
                    ExpenseAmount = e.ExpenseAmount,
                }).OrderByDescending(r => r.ExpenseAmount) // Use the correct property name
                    .ToList();
                ViewBag.ExpenseProgressJson = Newtonsoft.Json.JsonConvert.SerializeObject(ExpenseProgress);
                // room status
                var PricePerCare = _context.PricePerCares
                        .Where(p => p.IsActive == true && p.FloorId == null && p.MallId == mallId)
                        .ToList();

                var roomData = _context.Rooms
                    .Include(r => r.Floor)
                    .Where(r => r.Floor.MallId == mallId && r.IsActive == true && r.RoomDeactivateFlag == false && r.Status.ToString().Trim() == "Occupied")
                    .Select(r => new
                    {
                        RoomNumber = r.RoomNumber,
                        FloorNumber = r.Floor.FloorNumber,
                        RoomPrice = r.PricePercareFlag == false
                            ? _context.RoomPrices.Where(R => R.IsActive == true && R.RoomId == r.Id).Select(R => R.Price).FirstOrDefault()
                            : r.Care * PricePerCare.FirstOrDefault().Price
                    })
                    .OrderByDescending(r => r.RoomPrice) // Sorting by RoomPrice in descending order
                    .ToList();
                ViewBag.roomDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(roomData);

                // tenant aging 

                var tenantAging = _context.Rents
                    .Include(r => r.AppUser)
                    .Where(r => r.IsActive == true && r.MallId == mallId)
                    .Select(r => new
                    {
                        TenantName = r.AppUser.FirstName + " " + r.AppUser.LastName,
                        TenantPhone = r.AppUser.PhoneNumber,
                        RentalPeriodInDays = (now - r.RentalDate).TotalDays
                    })
                    .ToList() // Fetch the data first
                    .Select(r => new
                    {
                        r.TenantName,
                        r.TenantPhone,
                        RentalPeriodInDays = r.RentalPeriodInDays,
                        RentalPeriodString = FormatRentalPeriod(r.RentalPeriodInDays)
                    })
                    .OrderByDescending(r => r.RentalPeriodInDays) // Sorting by RentalPeriodInDays in descending order
                    .ToList();

                string FormatRentalPeriod(double totalDays)
                {
                    int days = (int)totalDays;
                    int years = days / 365;
                    days %= 365;
                    int months = days / 30;
                    days %= 30;
                    int weeks = days / 7;
                    days %= 7;

                    return $"{years} years / {months} months / {weeks} weeks / {days} days";
                }

                ViewBag.tenantAgingJson = Newtonsoft.Json.JsonConvert.SerializeObject(tenantAging);
                return View();
            }
            return View();
        }
    }
}
