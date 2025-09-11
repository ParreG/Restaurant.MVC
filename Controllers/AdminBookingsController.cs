using Microsoft.AspNetCore.Mvc;

namespace ResturantPG_MVC.Controllers
{
    public class AdminManageBookingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
