using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MallMinder.Data;
using MallMinder.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MallMinder.Controllers;
[Authorize(Roles = "Tenant")]

public class TenantDashbordController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    public TenantDashbordController(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser != null)
        {
            var Rent = _context.Rents.Where(r => r.IsActive == true && r.TenantId == currentUser.Id).FirstOrDefault();
            if (Rent != null)
            {
                var tenantPayments = _context.TenantPayments
                    .Where(t => t.IsActive && t.RentId == Rent.Id)
                    .Select(t => new { t.PaymentDate, t.PaidDate, t.Price })
                    .ToList();

                var paymentData = tenantPayments
                    .GroupBy(t => t.PaymentDate.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Price));

                var paidData = tenantPayments
                    .GroupBy(t => t.PaidDate.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Price));

                ViewBag.PaymentDataJson = JsonConvert.SerializeObject(paymentData);
                ViewBag.PaidDataJson = JsonConvert.SerializeObject(paidData);

                return View();
            }
        }
        return View();
    }
}
