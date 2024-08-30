
using System.Security.Cryptography.Xml;
using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MallMinder.Controllers;
[Authorize(Roles = "Admin")]

public class PaymentController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    public PaymentController(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }
    public async Task<IActionResult> PaymentList()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var mallId = _context.MallManagers
            .Where(m => m.OwnerId == currentUser.Id)
            .Select(m => m.MallId)
            .FirstOrDefault();
        var tenants = _context.Rents
            .Include(r => r.AppUser)
            .Include(r => r.Room)
            .Where(r => r.MallId == mallId && r.IsActive)
            .Select(r => new
            {
                Id = r.Id,
                RentalDate = r.RentalDate,
                Name = r.AppUser.FirstName + " " + r.AppUser.LastName,
                Phone = r.AppUser.PhoneNumber,
                Room = r.Room.RoomNumber
            })
            .ToList();
        var pendingPayments = new List<PendingPayment>();
        foreach (var tenant in tenants)
        {
            DateTime? PaymentDate = null; // Initialize PaymentDate as nullable DateTime
                                          // Retrieve the most recent payment date for the tenant
            var recentPayment = _context.TenantPayments
                    .Where(tp => tp.RentId == tenant.Id)
                    .OrderByDescending(tp => tp.PaymentDate)
                    .Select(tp => tp.PaymentDate)
                    .FirstOrDefault();
            DateTime referenceDate = DateTime.Now;
            int paymentFrequency = _context.Rents.Where(r => r.Id == tenant.Id).Select(r => r.PaymentDuration).FirstOrDefault();
            if (recentPayment == null || recentPayment == DateTime.MinValue)
            {
                PaymentDate = tenant.RentalDate;
                var pendingPayment = new PendingPayment
                {
                    TenantId = tenant.Id,
                    Name = tenant.Name,
                    Phone = tenant.Phone,
                    Room = tenant.Room,
                    PaymentDate = PaymentDate.Value,
                };
                pendingPayments.Add(pendingPayment);
            }
            else
            {
                PaymentDate = recentPayment;
            }
            int monthsDifference = (referenceDate.Year - PaymentDate.Value.Year) * 12 + (referenceDate.Month - PaymentDate.Value.Month);
            int rounds = monthsDifference / paymentFrequency;
            for (int i = 0; i < rounds; i++)
            {
                var pendingPayment2 = new PendingPayment
                {
                    TenantId = tenant.Id,
                    Name = tenant.Name,
                    Phone = tenant.Phone,
                    Room = tenant.Room,
                    PaymentDate = PaymentDate.Value.AddMonths(paymentFrequency * (i + 1))
                };
                pendingPayments.Add(pendingPayment2);
            }
        }
        ViewBag.PendingPayments = pendingPayments;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> PaymentList(TenantPaymentVM model)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var tenantPayment = new TenantPayment
            {
                RentId = model.RentId,
                PaymentDate = model.PaymentDate,
                ReferenceNumber = model.ReferenceNumber,
                CreatedBy = currentUser.Id,
                PaidDate = model.PaidDate,
                Price = model.Price,
            };
            _context.Add(tenantPayment);
            _context.SaveChanges();
            return RedirectToAction("Index", "Payment");
        }
        return View(model);
    }
    public async Task<IActionResult> Pay(int id, DateTime paymentDate)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        // Retrieve rent details including room information
        var rent = _context.Rents
            .Include(r => r.Room)
            .Where(r => r.Id == id)
            .FirstOrDefault();
        if (rent == null)
        {
            // Handle case where rent with given id is not found
            return NotFound();
        }
        var recentPayment = _context.TenantPayments
                        .Where(tp => tp.RentId == id)
                        .OrderByDescending(tp => tp.PaymentDate)
                        .Select(tp => tp.PaymentDate)
                        .FirstOrDefault();
        // Calculate PaymentDate based on recentPayment or RentalDate
        DateTime PaymentDate;
        if (recentPayment == null || recentPayment == DateTime.MinValue)
        {
            PaymentDate = rent.RentalDate.AddMonths(rent.PaymentDuration);
        }
        else
        {
            PaymentDate = recentPayment.AddMonths(rent.PaymentDuration);
        }
        double? totalPaymentAmount = 0;
        if (rent.Room.PricePercareFlag)
        {
            var PRICE_PER_CARE = _context.PricePerCares
                .Where(p => (p.FloorId == rent.Room.FloorId || p.FloorId == null) && p.IsActive == true)
                .Select(p => p.Price)
                .FirstOrDefault();
            totalPaymentAmount = rent.PaymentDuration * PRICE_PER_CARE * rent.Room.Care;
        }
        else
        {
            // Calculate payment amount based on room price from Room table
            double? roomPrice = _context.RoomPrices
                .Where(r => r.RoomId == rent.Room.Id)
                .Select(r => r.Price)
                .FirstOrDefault();
            totalPaymentAmount = roomPrice * rent.PaymentDuration * rent.Room.Care;
        }

        ViewBag.Id = rent.Id;
        ViewBag.PaymentDate = paymentDate;
        ViewBag.RoomNumber = rent.Room.RoomNumber;
        ViewBag.TotalPaymentAmount = totalPaymentAmount;
        return View();
    }
}
