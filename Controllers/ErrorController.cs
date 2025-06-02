using Microsoft.AspNetCore.Mvc;

namespace cce106_palit.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch(statusCode)
            {
                case 403:
                    ViewBag.ErrorMessage = "You are not authorized to access this page";
                    return View("Forbidden");

                case 404:
                    ViewBag.ErrorMessage = "Oops! Page not found.";
                    return View("NotFound");

                case 500:
                    ViewBag.ErrorMessage = "Sorry, something went wrong on the server.";
                    return View("ServerError");

                default:
                    ViewBag.ErrorMessage = "An unexpected error occurred.";
                    return View("GenericError");
            }
        }

        [Route("Error")]
        public IActionResult Error()
        {
            return View("ServerError");
        }
    }
}
