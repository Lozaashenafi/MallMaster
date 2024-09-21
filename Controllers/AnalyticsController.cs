using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMaster.Models.ViewModels;
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]

public class AnalyticsController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public AnalyticsController(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<IActionResult> MaintenanceAnalytics(DateTime? FromDate, DateTime? ToDate)
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
                .Where(r => r.Maintenance.MallId == mallId && ((FromDate != null && ToDate != null) ? r.Maintenance.RequestedDate.Value.Date >= FromDate.Value.Date && r.Maintenance.RequestedDate.Value.Date <= ToDate.Value.Date : r.Maintenance.RequestedDate.Value.Year == DateTime.Now.Year) && r.Maintenance.CompletedDate == null && r.IsActive == true)
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
    public async Task<IActionResult> CompletedMaintenanceAnalytics(DateTime? FromDate, DateTime? ToDate)
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
                .Where(r => r.MaintenanceStatusType.SysCode == 3 || r.MaintenanceStatusType.SysCode == 4 && ((FromDate != null && ToDate != null) ? r.Maintenance.CompletedDate.Value.Date >= FromDate.Value.Date && r.Maintenance.CompletedDate.Value.Date <= ToDate.Value.Date : r.Maintenance.CompletedDate.Value.Year == DateTime.Now.Year))
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
    public async Task<IActionResult> RoomAnalytics()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser != null)
        {
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                .Select(m => m.MallId)
                .FirstOrDefault();

            if (mallId != null)
            {
                var now = DateTime.Now;

                // room status
                var pricePerCare = _context.PricePerCares
                    .Where(p => p.IsActive == true && p.FloorId == null && p.MallId == mallId)
                    .FirstOrDefault();

                var roomData = _context.Rooms
                    .Include(r => r.Floor)
                    .Where(r => r.Floor != null && r.Floor.MallId == mallId && r.IsActive == true && r.RoomDeactivateFlag == false && r.Status.ToString().Trim() == "Occupied")
                    .ToList() // Fetch data from the database and then apply further logic in-memory
                    .Select(r => new
                    {
                        RoomNumber = r.RoomNumber,
                        FloorNumber = r.Floor.FloorNumber,
                        RoomPrice = r.PricePercareFlag == false
                            ? _context.RoomPrices.Where(R => R.IsActive == true && R.RoomId == r.Id).Select(R => R.Price).FirstOrDefault()
                            : (pricePerCare?.Price ?? 0) * r.Care
                    })
                    .OrderByDescending(r => r.RoomPrice) // Sorting by RoomPrice in descending order
                    .ToList();

                ViewBag.roomDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(roomData);
            }
        }

        return View();
    }


    public async Task<IActionResult> ExpenseAnalytics(DateTime? FromDate, DateTime? ToDate)
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
            var ExpenseProgress = _context.Expenses.Include(e => e.ExpenseType).Where(e => e.MallId == mallId && e.IsAcrive == true && ((FromDate != null && ToDate != null) ? e.ExpenseDate.Value.Date >= FromDate.Value.Date && e.ExpenseDate.Value.Date <= ToDate.Value.Date : e.ExpenseDate.Value.Year == DateTime.Now.Year)).Select(e => new
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
    public async Task<IActionResult> TenantAnalytics()
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
    public async Task<IActionResult> PaymentAnalytics()
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
                .Where(r => r.MallId == mallId && r.IsActive == true)
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
    public async Task<IActionResult> RoomOccupancyAnalytics(DateTime? FromDate, DateTime? ToDate)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser != null)
        {
            var mallId = _context.MallManagers
                .Where(m => m.OwnerId == currentUser.Id && m.IsActive)
                .Select(m => m.MallId)
                .FirstOrDefault();
            if (FromDate == null && ToDate == null)
            {
                FromDate = new DateTime(2024, 1, 1);
                ToDate = DateTime.Now;
            }
            var rooms = _context.Rooms.Include(r => r.Floor).Where(r => r.Floor.MallId == mallId).ToList();
            var roomOccupancies = _context.RoomOccupancies.Include(r => r.Room).ThenInclude(r => r.Floor).Where(r => r.Room.Floor.MallId == mallId).ToList();
            if (rooms == null || roomOccupancies == null)
            {
                ViewBag.ErrorMessage = "Could not retrieve room or room occupancy data.";
                return View();
            }

            TimeSpan? timeSpan = ToDate - FromDate;

            var totalDays = timeSpan.HasValue ? timeSpan.Value.TotalDays + 1 : 0;

            var roomOccupancyDetails = rooms.Select(room =>
 {
     if (room == null) return null;

     // Use a HashSet to track unique occupied days
     var occupiedDays = new HashSet<DateTime>();

     var occupancies = roomOccupancies
         .Where(ro => ro.RoomId == room.Id && ro.OccupiedDate <= ToDate && (ro.ReleasedDate == null || ro.ReleasedDate >= FromDate))
         .ToList();

     foreach (var occupancy in occupancies)
     {
         var startDate = occupancy.OccupiedDate < FromDate ? FromDate.Value : occupancy.OccupiedDate;
         var endDate = occupancy.ReleasedDate == null || occupancy.ReleasedDate > ToDate ? ToDate.Value : occupancy.ReleasedDate.Value;

         for (var date = startDate; date <= endDate; date = date.AddDays(1))
         {
             occupiedDays.Add(date);
         }
     }

     // Calculate total occupancy days and percentage
     var occupancyPercentage = totalDays > 0 ? (occupiedDays.Count / totalDays) * 100 : 0;

     return new RoomOccupancyDetail
     {
         RoomNumber = room.RoomNumber,
         FloorNumber = room.Floor.FloorNumber,
         OccupancyPercentage = occupancyPercentage
     };
 })
 .Where(detail => detail != null) // Filter out null entries
 .OrderByDescending(detail => detail.OccupancyPercentage)
 .ToList();


            ViewBag.RoomOccupancyDetails = roomOccupancyDetails;

            return View();
        }
        return View();
    }


}
