using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.DTOs.TableDTOs;
using ResturantPG_MVC.Extensions;
using ResturantPG_MVC.ViewModel;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ResturantPG_MVC.Controllers
{
    [Route("AdminTables")]
    [IgnoreAntiforgeryToken]

    public class AdminTablesController : Controller
    {
        private readonly HttpClient httpClient;

        public AdminTablesController(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient("ResturangApi");
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> AdminTables()
        {
            httpClient.AddAuthHeader(HttpContext);

            var resp = await httpClient.GetAsync("Table/GetAllTables");

            if (!resp.IsSuccessStatusCode)
                return View("~/Views/AdminPanel/AdminTables.cshtml", new List<TableVM>());

            var apiTables = await resp.Content.ReadFromJsonAsync<List<TableDTO>>() ?? new();

            var vm = apiTables.Select((t, i) => new TableVM
            {
                Id = t.Table_Id,
                Number = t.Number,
                Capacity = t.Capacity,
                X = t.X ?? (150 + (i * 200)),
                Y = t.Y ?? 200,
                IsAvailable = t.IsAvailable
            }).ToList();

            return View("~/Views/AdminPanel/AdminTables.cshtml", vm);
        }


        [HttpPost("SaveLayout")]
        [Authorize]
        public async Task<IActionResult> SaveLayout([FromBody] UpdatePositionsDTO dto)
        {
            httpClient.AddAuthHeader(HttpContext);

            var tried = new[]
            {
                "Table/UpdatePositions",
                "Table/SavePositions"
            };

            foreach (var url in tried)
            {
                var apiResponse = await httpClient.PutAsJsonAsync(url, dto);
                var body = await apiResponse.Content.ReadAsStringAsync();
                if (apiResponse.IsSuccessStatusCode) return Ok(body);
                if (apiResponse.StatusCode != HttpStatusCode.NotFound)
                    return StatusCode((int)apiResponse.StatusCode, body);
            }

            return NotFound("Inget SaveLayout-endpoint hittades i API:t.");
        }

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrUpdateTableDTO dto)
        {
            httpClient.AddAuthHeader(HttpContext);

            var tried = new[]
            {
                "Table/CreateTable",
                "Table/AddNewTable",
                "Table/AddTable",
                "Table/Create",
                "Table"
            };

            foreach (var url in tried)
            {
                var apiResponse = await httpClient.PostAsJsonAsync(url, dto);
                var body = await apiResponse.Content.ReadAsStringAsync();
                if (apiResponse.IsSuccessStatusCode) return Ok(body);
                if (apiResponse.StatusCode != HttpStatusCode.NotFound)
                    return StatusCode((int)apiResponse.StatusCode, body);
            }

            return NotFound("Inget Create-endpoint hittades i API:t.");
        }

        [HttpPost("Update/{number:int}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int number, [FromBody] CreateOrUpdateTableDTO dto)
        {
            httpClient.AddAuthHeader(HttpContext);

            var tried = new[]
            {
                $"Table/UpdateTable/{number}",
                $"Table/Update/{number}",
                $"Table/{number}"
            };

            foreach (var url in tried)
            {
                var apiResponse = await httpClient.PutAsJsonAsync(url, dto);
                var body = await apiResponse.Content.ReadAsStringAsync();
                if (apiResponse.IsSuccessStatusCode) return Ok(body);
                if (apiResponse.StatusCode != HttpStatusCode.NotFound)
                    return StatusCode((int)apiResponse.StatusCode, body);
            }

            return NotFound("Inget Update-endpoint hittades i API:t.");
        }

        [HttpDelete("Delete/{number:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int number)
        {
            httpClient.AddAuthHeader(HttpContext);

            var tried = new[]
            {
                $"Table/DeleteTable/{number}",
                $"Table/Delete/{number}",
                $"Table/{number}"
            };

            foreach (var url in tried)
            {
                var apiResponse = await httpClient.DeleteAsync(url);
                var body = await apiResponse.Content.ReadAsStringAsync();

                if (apiResponse.IsSuccessStatusCode) return Ok();

                // Fånga vanliga texter från EF/SQL när FK stoppar delete
                var isFkConflict =
                    body.Contains("FK_Bookings", StringComparison.OrdinalIgnoreCase) ||
                    body.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase) ||
                    body.Contains("foreign key", StringComparison.OrdinalIgnoreCase);

                if ((int)apiResponse.StatusCode >= 500 && isFkConflict)
                {
                    
                    return StatusCode(StatusCodes.Status409Conflict, "TABLE_HAS_BOOKINGS");
                }

                if (apiResponse.StatusCode != HttpStatusCode.NotFound)
                    return StatusCode((int)apiResponse.StatusCode, body);
            }

            return NotFound("Inget Delete-endpoint hittades i API:t.");
        }
    }
}
