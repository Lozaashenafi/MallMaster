using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
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

            }
            return View();
        }
        public async Task<IActionResult> ComplitedMaintenance()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                var now = DateTime.Now;
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
            }
            return View();
        }
        public async Task<IActionResult> Room()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                var now = DateTime.Now;
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
            }
            return View();
        }
        public async Task<IActionResult> Expense()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                var now = DateTime.Now;
                var twelveMonthsAgo = now.AddMonths(-12);
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

                return View();
            }
            return View();
        }
        public async Task<IActionResult> Tenant()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                var now = DateTime.Now;
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
        public async Task<IActionResult> Payment()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var mallId = _context.MallManagers
                    .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                    .Select(m => m.MallId)
                    .FirstOrDefault();
                var now = DateTime.Now;

                var tenants = _context.Rents
                    .Include(r => r.AppUser)
                    .Include(r => r.Room)
                    .ThenInclude(r => r.Floor)
                    .Where(r => r.MallId == mallId && r.IsActive)
                    .Select(r => new
                    {
                        Id = r.Id,
                        RentalDate = r.RentalDate,
                        Name = r.AppUser.FirstName + " " + r.AppUser.LastName,
                        RoomNumber = r.Room.RoomNumber,
                        FloorId = r.Room.FloorId,
                        PaymentDuration = r.PaymentDuration,
                        Care = r.Room.Care,
                        PricePercareFlag = r.Room.PricePercareFlag,
                        RoomId = r.Room.Id
                    })
                    .ToList();

                var paymentSummaries = new List<PaymentSummery>();
                var referenceDate = DateTime.Now;

                foreach (var tenant in tenants)
                {
                    DateTime? recentPayment = _context.TenantPayments
                        .Where(tp => tp.RentId == tenant.Id)
                        .OrderByDescending(tp => tp.PaymentDate)
                        .Select(tp => tp.PaymentDate)
                        .FirstOrDefault();
                    if (recentPayment == DateTime.MinValue)
                    {
                        recentPayment = null;
                    }

                    DateTime paymentDate = recentPayment ?? tenant.RentalDate;
                    int paymentFrequency = tenant.PaymentDuration; // Using the value from tenant object

                    int monthsDifference = (referenceDate.Year - paymentDate.Year) * 12 + (referenceDate.Month - paymentDate.Month);
                    int rounds = monthsDifference / paymentFrequency;

                    // Ensure rounds is not less than 0
                    rounds = Math.Max(rounds, 0);

                    var pricePerCare = _context.PricePerCares
                        .Where(p => p.FloorId == tenant.FloorId || (p.FloorId == null && p.IsActive == true))
                        .Select(p => p.Price)
                        .FirstOrDefault();

                    double? totalPaymentAmount = tenant.PricePercareFlag
                        ? tenant.PaymentDuration * pricePerCare * tenant.Care * (rounds > 0 ? rounds : 1)
                        : _context.RoomPrices
                            .Where(p => p.RoomId == tenant.RoomId)
                            .Select(p => p.Price)
                            .FirstOrDefault() * tenant.PaymentDuration * (rounds > 0 ? rounds : 1);

                    string status = "";
                    if (monthsDifference > paymentFrequency)
                    {
                        status = "overdue";
                    }
                    else
                    {
                        int defferenceDate = (paymentFrequency * 30) - (monthsDifference * 30);
                        status = defferenceDate > 15 ? "Paid" : "upcoming";
                    }

                    var paymentSummary = new PaymentSummery
                    {
                        TenantName = tenant.Name,
                        RentalDate = tenant.RentalDate.Date, // Format the date to only include the day
                        RoomNumber = tenant.RoomNumber,
                        UnpaidRounds = rounds,
                        PaymentAmount = totalPaymentAmount,
                        PaymentStatus = status
                    };

                    paymentSummaries.Add(paymentSummary);
                }

                // Sort by UnpaidRounds in descending order
                paymentSummaries = paymentSummaries.OrderByDescending(p => p.UnpaidRounds).ToList();

                ViewBag.paymentSummariesJson = Newtonsoft.Json.JsonConvert.SerializeObject(paymentSummaries);
                return View();
            }
            return View();
        }


    }
}
