using Cabinet_Medical.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class AccountController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public AccountController(CabinetMedicalContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                    u.PasswordHash == password &&
                    u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Nom d'utilisateur ou mot de passe incorrect";
                return View();
            }

            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("Username", user.Username);

            // Redirection selon rôle
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ==============================
        // INSCRIPTION (GET)
        // ==============================
        public IActionResult Register()
        {
            return View();
        }

        // ==============================
        // INSCRIPTION (POST) - PATIENT
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string username, 
            string password, 
            string email,
            string nom,
            string prenom,
            string telephone,
            string adresse,
            DateTime? dateNaissance,
            string antecedentsMedicaux)
        {
            // Validation des champs obligatoires
            if (string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(nom) ||
                string.IsNullOrWhiteSpace(prenom) ||
                string.IsNullOrWhiteSpace(telephone) ||
                string.IsNullOrWhiteSpace(adresse) ||
                string.IsNullOrWhiteSpace(antecedentsMedicaux))
            {
                ViewBag.Error = "Tous les champs sont obligatoires.";
                return View();
            }

            // Vérifier si le username existe déjà
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Ce nom d'utilisateur est déjà utilisé.";
                return View();
            }

            // Vérifier si l'email existe déjà
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Cet email est déjà utilisé.";
                return View();
            }

            // Créer l'utilisateur
            var user = new Models.User
            {
                Username = username,
                PasswordHash = password, // À améliorer avec hashage
                Email = email,
                Role = "Patient",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Créer le patient
            var patient = new Models.Patient
            {
                UserId = user.Id,
                Nom = nom,
                Prenom = prenom,
                Telephone = telephone,
                Adresse = adresse,
                DateNaissance = dateNaissance,
                AntecedentsMedicaux = antecedentsMedicaux
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
            return RedirectToAction("Login");
        }
    }
}
