using Cabinet_Medical.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Cabinet_Medical.Controllers
{
    public class PatientDashboardController : RoleController
    {
        public PatientDashboardController() : base("Patient") { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
