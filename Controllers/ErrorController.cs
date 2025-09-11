using Microsoft.AspNetCore.Mvc;

namespace ResturantPG_MVC.Controllers
{
    public class ErrorController : Controller
    {
        [Route("error/500")]
        public IActionResult Error500()
        {
            Response.StatusCode = 500;
            return View("~/Views/Shared/Error.cshtml");
        }

        [Route("error/{statusCode}")]
        public IActionResult ErrorByCode(int statusCode)
        {
            Response.StatusCode = statusCode;

            return statusCode switch
            {
                401 => View("~/Views/Shared/Unauthorized.cshtml"),
                403 => View("~/Views/Shared/Forbidden.cshtml"),
                404 => View("~/Views/Shared/NotFound.cshtml"),
                _ => View("~/Views/Shared/Error.cshtml")
            };
        }
    }
}
