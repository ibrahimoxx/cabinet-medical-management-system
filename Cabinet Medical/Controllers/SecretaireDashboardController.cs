using Cabinet_Medical.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Cabinet_Medical.Controllers
{
    public class SecretaireDashboardController : RoleController
    {
        public SecretaireDashboardController() : base("Secretaire") { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
