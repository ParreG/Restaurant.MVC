using Microsoft.AspNetCore.Mvc;
using ResturantPG_MVC.DTOs.AdminDtos;
using ResturantPG_MVC.Extensions;
using ResturantPG_MVC.ViewModel;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;

public class AdminController : Controller
{
    private readonly HttpClient httpClient;

    public AdminController(IHttpClientFactory clientFactory)
    {
        httpClient = clientFactory.CreateClient("ResturangApi");
    }

    [HttpGet("Login")]
    public IActionResult Login() => View(new AdminAuthVM());

    [HttpPost("Login")]
    public async Task<IActionResult> Login(AdminAuthVM model)
    {
        var loginDto = new AdminLoginDTO
        {
            Username = model.Login.Username,
            Password = model.Login.Password
        };

        var response = await httpClient.PostAsJsonAsync("Auth/Login", loginDto);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Felaktiga uppgifter!");
            return View(model);
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseVM>();
        var token = result!.Token;

        HttpContext.Session.SetToken(token);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Login.Username ?? string.Empty)
        };

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaims = jwt.Claims.Where(c =>
            c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles");

        foreach (var rc in roleClaims)
        {
            foreach (var r in rc.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                claims.Add(new Claim(ClaimTypes.Role, r));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Register(AdminAuthVM model)
    {
        var registerDto = new AdminRegisterDTO
        {
            Email = model.Register.Email,
            UserName = model.Register.UserName,
            Password = model.Register.Password,
            RegistrationCode = model.Register.RegistrationCode
        };

        var response = await httpClient.PostAsJsonAsync("Auth/Register", registerDto);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Konto skapat! Du kan nu logga in.";
            return RedirectToAction("Login");
        }

        ModelState.AddModelError("", "Registreringen misslyckades. Försök igen.");
        model.ShowRegister = true;
        return View("Login", model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.ClearToken();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Denied() => View("Forbidden"); 
}
