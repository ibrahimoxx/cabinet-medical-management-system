using Cabinet_Medical.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class ProfileController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public ProfileController(CabinetMedicalContext context)
        {
            _context = context;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Medecin)
                .Include(u => u.Secretaire)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Medecin)
                .Include(u => u.Secretaire)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Cabinet_Medical.Models.User model)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Medecin)
                .Include(u => u.Secretaire)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            // Update common fields
            user.Email = model.Email;

            // Role-specific updates
            if (user.Role == "Patient" && user.Patient != null)
            {
                user.Patient.Nom = model.Patient?.Nom;
                user.Patient.Prenom = model.Patient?.Prenom;
                user.Patient.Telephone = model.Patient?.Telephone;
                user.Patient.Adresse = model.Patient?.Adresse;
            }

            if (user.Role == "Medecin" && user.Medecin != null)
            {
                user.Medecin.Nom = model.Medecin?.Nom;
                user.Medecin.Prenom = model.Medecin?.Prenom;
                user.Medecin.Telephone = model.Medecin?.Telephone;
            }

            if (user.Role == "Secretaire" && user.Secretaire != null)
            {
                user.Secretaire.Nom = model.Secretaire?.Nom;
                user.Secretaire.Prenom = model.Secretaire?.Prenom;
                user.Secretaire.Telephone = model.Secretaire?.Telephone;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
