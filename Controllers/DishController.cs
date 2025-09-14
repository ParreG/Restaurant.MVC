using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.Extensions;
using ResturantPG_MVC.Models;
using ResturantPG_MVC.ViewModel;
using System.Threading.Tasks;

namespace ResturantPG_MVC.Controllers
{
    public class DishController : Controller
    {
        private readonly HttpClient httpClient;

        public DishController(IHttpClientFactory _clientFactory)
        {
            httpClient = _clientFactory.CreateClient("ResturangApi");
        }

        public async Task<IActionResult> Index()
        {
            var response = await httpClient.GetAsync("Dish/GetAllDishes");

            var dishes = await response.Content.ReadFromJsonAsync<List<Dish>>();

            return View(dishes);
        }

        public IActionResult CreateDish()
        {
            httpClient.AddAuthHeader(HttpContext);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDish(CreateDishVM newDish)
        {
            httpClient.AddAuthHeader(HttpContext);
            if (!ModelState.IsValid)
            {
                return View(newDish);
            }

            var response = await httpClient.PostAsJsonAsync("Dish/AddDish", newDish);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Kunde inte spara maträtten. Försök igen.");
                return View(newDish);
            }

            return RedirectToAction("Index");
        }
    }
}
