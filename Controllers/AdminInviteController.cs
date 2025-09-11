using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.ViewModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ResturantPG_MVC.Extensions;

namespace ResturantPG_MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("admin/invites")]
    public class AdminInviteController : Controller
    {
        private readonly HttpClient httpClient;

        public AdminInviteController(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient("ResturangApi");
        }

        // GET /admin/invites
        [HttpGet("")]
        [Authorize]
        public IActionResult AdminInvites()
        {
            return View("~/Views/AdminPanel/AdminInvites.cshtml", new AdminInviteVM());
        }

        [HttpPost("CreateAdminInvites")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateAdminInvites()
        {
            var token = HttpContext.Session.GetToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.PostAsync("Auth/CreateInvite", content: null);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return View("~/Views/AdminPanel/AdminInvites.cshtml",
                    new AdminInviteVM { Message = "Inte inloggad eller sessionen har gått ut." });
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return View("~/Views/AdminPanel/AdminInvites.cshtml",
                    new AdminInviteVM { Message = "Du saknar behörighet (SuperAdmin krävs)." });
            }
            if (!response.IsSuccessStatusCode)
            {
                return View("~/Views/AdminPanel/AdminInvites.cshtml",
                    new AdminInviteVM { Message = "Kunde inte skapa invite-kod." });
            }

            var inviteCode = await response.Content.ReadFromJsonAsync<Guid>();

            return View("~/Views/AdminPanel/AdminInvites.cshtml", new AdminInviteVM
            {
                InviteCode = inviteCode,
                Message = "Ny invite-kod skapad."
            });
        }
    }
}
