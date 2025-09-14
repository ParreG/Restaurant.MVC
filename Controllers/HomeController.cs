using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.Models;

namespace ResturantPG_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;

        public HomeController(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("ResturangApi");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("Dish/GetAllDishes");
            var dishes = await response.Content.ReadFromJsonAsync<List<Dish>>();


            var popular = dishes?
                .Where(d => d.IsPopular)
                .Take(8) 
                .ToList() ?? new List<Dish>();

            return View(popular);
        }
    }
}
