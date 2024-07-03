using Microsoft.AspNetCore.Mvc;

namespace MallMinder.Controllers
{ 
    public class RoomController : Controller
    {
     
        public IActionResult Index()
        {
            return View();
        }
    }
}