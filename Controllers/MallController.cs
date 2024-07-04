using MallMinder.Data;
using MallMinder.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MallMinder.Controllers
{
    public class MallController : Controller
    {
        private readonly AppDbContext _context;

        public MallController(AppDbContext context)
        {
            _context = context;
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
                _context.Mall.Add(mall);
                _context.SaveChanges();

                // Redirect to a success page or another action
                return RedirectToAction("Index", "System"); // Example redirect to Home/Index
            }

            // If model state is not valid, return to the same view with errors
            return View(mall);
        }
    }
}
