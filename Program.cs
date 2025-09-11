using Microsoft.AspNetCore.Authentication.Cookies;

namespace ResturantPG_MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient("ResturangApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7278/api/");
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Om man inte är inloggad visa vår 401-sida
                    options.LoginPath = "/error/401";

                    // Om man saknar rättighet visa vår 403-sida
                    options.AccessDeniedPath = "/error/403";


                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = ctx =>
                        {
                            // Skicka alltid till 401-sida
                            ctx.Response.Redirect("/error/401");
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = ctx =>
                        {
                            // Skicka alltid till 403-sida
                            ctx.Response.Redirect("/error/403");
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                // 500-fel hamnar här
                app.UseExceptionHandler("/error/500");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // 401/403/404/… -> /error/{statusCode}
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
