using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace ResturantPG_MVC.Extensions
{
    public static class SessionExtensions
    {
        public static void SetToken(this ISession session, string token, int expireMinutes = 30)
        {
            session.SetString("JWToken", token);
            session.SetString("JWToken_Expire", DateTime.Now.AddMinutes(expireMinutes).ToString());
        }

        public static string? GetToken(this ISession session)
        {
            return session.GetString("JWToken");
        }

        public static void ClearToken(this ISession session)
        {
            session.Remove("JWToken");
            session.Remove("JWToken_Expire");
        }
    }

    public static class HttpClientExtensions
    {
        public static void AddAuthHeader(this HttpClient client, HttpContext context)
        {
            var token = context.Session.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
