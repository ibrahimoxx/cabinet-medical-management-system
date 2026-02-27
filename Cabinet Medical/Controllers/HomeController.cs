using Microsoft.AspNetCore.Mvc;

namespace Cabinet_Medical.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public IActionResult Privacy()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
