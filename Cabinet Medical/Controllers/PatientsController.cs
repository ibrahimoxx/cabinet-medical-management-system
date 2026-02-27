using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class PatientsController : RoleController
    {
        private readonly CabinetMedicalContext _context;

        public PatientsController(CabinetMedicalContext context)
            : base("Secretaire")
        {
            _context = context;
        }

        // ==============================
        // CONSULTER PATIENTS
        // ==============================
        public async Task<IActionResult> Index(string search = "")
        {
            IQueryable<Patient> query = _context.Patients
                .Include(p => p.User)
                .Include(p => p.DossierMedical);

            // Filtre par nom ou prénom
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => 
                    p.Nom.Contains(search) || 
                    p.Prenom.Contains(search) ||
                    (p.Nom + " " + p.Prenom).Contains(search) ||
                    (p.User != null && p.User.Email.Contains(search)) ||
                    (p.Telephone != null && p.Telephone.Contains(search)));
            }

            var patients = await query
                .OrderBy(p => p.Nom)
                .ToListAsync();

            // Passer la valeur du filtre à la vue
            ViewBag.Search = search;

            return View(patients);
        }

        // ==============================
        // AJOUTER PATIENT (GET)
        // ==============================
        [HttpGet]
        public IActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ==============================
        // AJOUTER PATIENT (POST) - CRÉATION UTILISATEUR + PATIENT
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string returnUrl = null)
        {
            // Récupérer les valeurs du formulaire
            string username = Request.Form["Username"].ToString().Trim();
            string email = Request.Form["Email"].ToString().Trim();
            string password = Request.Form["Password"].ToString();
            string nom = Request.Form["Nom"].ToString().Trim();
            string prenom = Request.Form["Prenom"].ToString().Trim();
            string telephone = Request.Form["Telephone"].ToString().Trim();
            string adresse = Request.Form["Adresse"].ToString().Trim();
            string antecedentsMedicaux = Request.Form["AntecedentsMedicaux"].ToString().Trim();
            
            // Validation
            bool hasError = false;

            // Validation Username
            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("Username", "Le nom d'utilisateur est requis.");
                hasError = true;
            }
            else if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Ce nom d'utilisateur existe déjà.");
                hasError = true;
            }

            // Validation Email
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("Email", "L'email est requis.");
                hasError = true;
            }
            else if (!email.Contains("@"))
            {
                ModelState.AddModelError("Email", "L'email n'est pas valide.");
                hasError = true;
            }
            else if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "Cet email existe déjà.");
                hasError = true;
            }

            // Validation Password
            if (string.IsNullOrEmpty(password) || password.Length < 4)
            {
                ModelState.AddModelError("Password", "Le mot de passe est requis (minimum 4 caractères).");
                hasError = true;
            }

            // Validation Nom
            if (string.IsNullOrEmpty(nom))
            {
                ModelState.AddModelError("Nom", "Le nom est requis.");
                hasError = true;
            }

            // Validation Prénom
            if (string.IsNullOrEmpty(prenom))
            {
                ModelState.AddModelError("Prenom", "Le prénom est requis.");
                hasError = true;
            }

            // Validation Téléphone (optionnel mais recommandé)
            if (string.IsNullOrEmpty(telephone))
            {
                telephone = "";
            }

            // Si erreurs, retourner à la vue
            if (hasError)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Créer l'utilisateur
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = password, // ⚠️ À hasher en production
                Role = "Patient",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Sauvegarder pour obtenir l'ID

            // Créer le patient
            DateTime? dateNaissance = null;
            if (!string.IsNullOrEmpty(Request.Form["DateNaissance"].ToString()))
            {
                if (DateTime.TryParse(Request.Form["DateNaissance"].ToString(), out DateTime date))
                {
                    dateNaissance = date;
                }
            }

            var patient = new Patient
            {
                UserId = user.Id,
                Nom = nom,
                Prenom = prenom,
                DateNaissance = dateNaissance,
                Adresse = string.IsNullOrEmpty(adresse) ? null : adresse,
                Telephone = telephone,
                AntecedentsMedicaux = string.IsNullOrEmpty(antecedentsMedicaux) ? null : antecedentsMedicaux
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient créé avec succès (utilisateur + profil).";
            
            // Rediriger vers returnUrl si fourni, avec le patientId en paramètre pour pré-sélection
            if (!string.IsNullOrEmpty(returnUrl))
            {
                var separator = returnUrl.Contains("?") ? "&" : "?";
                var redirectUrl = $"{returnUrl}{separator}patientId={patient.Id}";
                return Redirect(redirectUrl);
            }
            
            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // MODIFIER PATIENT (GET)
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            
            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // ==============================
        // MODIFIER PATIENT (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Nom,Prenom,DateNaissance,Adresse,Telephone,AntecedentsMedicaux")] Patient patient)
        {
            if (id != patient.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Patient modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(patient);
        }

        // ==============================
        // DÉTAILS PATIENT
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.DossierMedical)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // ==============================
        // SUPPRIMER PATIENT (GET)
        // ==============================
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // ==============================
        // SUPPRIMER PATIENT (POST)
        // ==============================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Patient supprimé avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}

