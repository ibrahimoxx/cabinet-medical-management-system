using Cabinet_Medical.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Cabinet_Medical.Controllers
{
    public class MedecinDashboardController : RoleController
    {
        public MedecinDashboardController() : base("Medecin") { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
