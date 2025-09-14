using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.Extensions;
using ResturantPG_MVC.Models;
using ResturantPG_MVC.ViewModel;
using System.Text.Json;

namespace ResturantPG_MVC.Controllers
{
    public class AdminDishesController : Controller
    {
        private readonly HttpClient httpClient;

        public AdminDishesController(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient("ResturangApi");
        }

        [HttpGet("GetAllDishes")]
        public async Task<IActionResult> AdminDishes() // ingen authorize då den används i menyn!
        {
            var response = await httpClient.GetAsync("Dish/GetAllDishes");

            if (!response.IsSuccessStatusCode)
            {
                // Om API returnerar 404 eller något annat → returnera tom lista
                return View(new List<Dish>());
            }

            var content = await response.Content.ReadAsStringAsync();

            List<Dish>? dishes;
            try
            {
                dishes = JsonSerializer.Deserialize<List<Dish>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // Om det inte går att tolka → returnera tom lista istället
                dishes = new List<Dish>();
            }

            return View(dishes ?? new List<Dish>());

        }

        [HttpGet("UpdateDish/{id}", Name = "UpdateDish")]
        [Authorize]
        public async Task<IActionResult> AdminUpdateDish(int id)
        {
            httpClient.AddAuthHeader(HttpContext);


            var response = await httpClient.GetAsync($"Dish/GetDishById/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound(); 
            }

            var dish = await response.Content.ReadFromJsonAsync<DishVM>();
            return View(dish);
        }

        [Authorize]
        [HttpPost("UpdateDish/{id}", Name = "UpdateDish")]
        public async Task<IActionResult> AdminUpdateDish(int id, DishVM dish)
        {
            httpClient.AddAuthHeader(HttpContext);

            var response = await httpClient.PutAsJsonAsync($"Dish/UpdateDish/{id}", dish);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminDishes");
            }

            ModelState.AddModelError("", "Uppdateringen misslyckades.");
            return View(dish);
        }

        [HttpGet("DeleteDish/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteDish(int id)
        {
            httpClient.AddAuthHeader(HttpContext);

            var response = await httpClient.DeleteAsync($"Dish/DeleteDish/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminDishes"); 
            }

            ModelState.AddModelError("", "Borttagningen misslyckades.");
            return RedirectToAction("AdminDishes"); 
        }




    }
}
