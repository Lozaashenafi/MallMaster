
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MallMaster.Controllers
{
    public class MallController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddMall()
        {
            return View();
        }
    }
}