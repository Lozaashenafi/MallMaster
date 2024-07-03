using Microsoft.AspNetCore.Mvc;

namespace MallMinder.Controllers
{
    public class TenantController : Controller
    {
        // GET: Tenant/AddTenant
        public IActionResult AddTenant()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
