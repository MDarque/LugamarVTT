using Microsoft.AspNetCore.Mvc;

namespace LugamarVTT.Controllers
{
    /// <summary>
    /// Simple home controller that provides a landing page for the application.
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Optionally add an error action for fallback error handling
        public IActionResult Error()
        {
            return View();
        }
    }
}