using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MallMinder.Controllers;
[Authorize(Roles = "SystemAdmin")]

// [Authorize(Roles = "SystemAdmin")]
public class SystemController : Controller
{
    private readonly ILogger<SystemController> _logger;

    public SystemController(ILogger<SystemController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error");
    }
}

