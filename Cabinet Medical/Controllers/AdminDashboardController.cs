using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class AdminDashboardController : RoleController
    {
        private readonly CabinetMedicalContext _context;

        public AdminDashboardController(CabinetMedicalContext context) : base("Admin") 
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalMedecins = await _context.Medecins.CountAsync();
            return View();
        }
    }
}
