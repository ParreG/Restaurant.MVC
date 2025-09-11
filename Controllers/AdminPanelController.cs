using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ResturantPG_MVC.Controllers
{
    [Authorize] 
    public class AdminPanelController : Controller
    {

        [HttpGet]
        public IActionResult AdminPanel()
        {
            return View(); 
        }
    }
}
