using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class AlertesController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public AlertesController(CabinetMedicalContext context)
        {
            _context = context;
        }

        // ==============================
        // CONSULTER LES ALERTES
        // ==============================
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var alertes = await _context.Alertes
                .Include(a => a.RendezVous)
                    .ThenInclude(r => r.Medecin)
                .Include(a => a.RendezVous)
                    .ThenInclude(r => r.Patient)
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.DateCreation)
                .ToListAsync();

            return View(alertes);
        }

        // ==============================
        // MARQUER ALERTE COMME LUE
        // ==============================
        [HttpPost]
        public async Task<IActionResult> MarquerLue([FromBody] int id)
        {
            var alerte = await _context.Alertes.FindAsync(id);
            if (alerte != null)
            {
                alerte.EstLue = true;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        // ==============================
        // API : NOMBRE D'ALERTES NON LUES
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return Json(new { count = 0 });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Json(new { count = 0 });

            var count = await _context.Alertes
                .CountAsync(a => a.UserId == user.Id && !a.EstLue);

            return Json(new { count });
        }
    }
}

