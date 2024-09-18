using MallMinder.Data;
using MallMinder.Models;
using MallMinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace MallMinder.Controllers;
[Authorize(Roles = "SystemAdmin")]

public class MallController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public MallController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AddMall()
    {
        return View();
    }

    [HttpPost]
    public IActionResult AddMall(Mall mall)
    {
        if (ModelState.IsValid)
        {
            // Set AddedDate to current date/time
            mall.AddedDate = DateTime.Now;

            // Add mall to DbContext and save changes
            _context.Malls.Add(mall);
            _context.SaveChanges();

            // Create floors based on TotalFloors
            for (int i = 1; i <= mall.TotalFloors; i++)
            {
                var floor = new Floor
                {
                    FloorNumber = $"{i}{GetOrdinalSuffix(i)} Floor",
                    MallId = mall.Id
                };
                _context.Floors.Add(floor);
            }
            _context.SaveChanges();

            // Redirect to a success page or another action
            return RedirectToAction("Index", "System"); // Example redirect to Home/Index
        }

        // If model state is not valid, return to the same view with errors
        return View(mall);
    }

    // Helper method to get ordinal suffix (st, nd, rd, th)
    private string GetOrdinalSuffix(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 13)
        {
            return "th";
        }

        switch (number % 10)
        {
            case 1:
                return "Ground";
            case 2:
                return "st";
            case 3:
                return "nd";
            case 4:
                return "rd";
            default:
                return "th";
        }
    }

}

