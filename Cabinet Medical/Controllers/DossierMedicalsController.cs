using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class DossierMedicalsController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public DossierMedicalsController(CabinetMedicalContext context)
        {
            _context = context;
        }

        // Vérification du rôle
        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Secretaire" || role == "Medecin" || role == "Patient";
        }

        // ==============================
        // CONSULTER DOSSIERS MÉDICAUX (SECRÉTAIRE)
        // ==============================
        public async Task<IActionResult> Index(string searchPatient = "")
        {
            var role = HttpContext.Session.GetString("UserRole");
            
            if (role != "Secretaire" && role != "Medecin")
                return RedirectToAction("MonDossier");

            IQueryable<DossierMedical> query = _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User);

            // Filtre par nom de patient
            if (!string.IsNullOrEmpty(searchPatient))
            {
                query = query.Where(d => 
                    d.Patient.Nom.Contains(searchPatient) || 
                    d.Patient.Prenom.Contains(searchPatient) ||
                    (d.Patient.Nom + " " + d.Patient.Prenom).Contains(searchPatient));
            }

            var dossiers = await query
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();

            // Passer la valeur du filtre à la vue
            ViewBag.SearchPatient = searchPatient;

            return View(dossiers);
        }

        // ==============================
        // MON DOSSIER MÉDICAL (PATIENT)
        // ==============================
        public async Task<IActionResult> MonDossier()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("Username");
            
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
                return RedirectToAction("Login", "Account");

            Patient patient = null;

            if (role == "Patient")
            {
                patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            }
            else if (role == "Medecin" || role == "Secretaire")
            {
                // Permettre la consultation d'un dossier médical via l'ID du patient
                return RedirectToAction("Index");
            }

            if (patient == null)
            {
                TempData["ErrorMessage"] = "Aucun dossier médical trouvé.";
                return RedirectToAction("Index", "Dashboard");
            }

            var dossier = await _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.PatientId == patient.Id);

            if (dossier == null)
            {
                TempData["InfoMessage"] = "Votre dossier médical n'a pas encore été créé.";
                return View((DossierMedical)null);
            }

            // Charger les consultations associées
            var consultations = await _context.Consultations
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .Where(c => c.DossierMedicalId == dossier.Id)
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            ViewBag.Consultations = consultations;

            return View(dossier);
        }

        // ==============================
        // CONSULTER DOSSIER (MÉDECIN/SECRÉTAIRE)
        // ==============================
        public async Task<IActionResult> Consulter(int? id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var dossier = await _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dossier == null)
                return NotFound();

            // Charger les consultations
            var consultations = await _context.Consultations
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .Where(c => c.DossierMedicalId == dossier.Id)
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            ViewBag.Consultations = consultations;

            return View(dossier);
        }

        // ==============================
        // CRÉER DOSSIER MÉDICAL (GET) - SECRÉTAIRE
        // ==============================
        public async Task<IActionResult> Create(int? patientId = null)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            // Si patientId est fourni, pré-sélectionner ce patient
            if (patientId.HasValue)
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.DossierMedical)
                    .FirstOrDefaultAsync(p => p.Id == patientId.Value);

                if (patient != null && patient.DossierMedical == null)
                {
                    ViewBag.PatientId = patient.Id;
                    ViewBag.PatientName = $"{patient.Nom} {patient.Prenom}";
                }
            }

            // Patients sans dossier médical
            var patientsAvecDossier = await _context.DossierMedicals
                .Select(d => d.PatientId)
                .ToListAsync();

            ViewBag.Patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive && !patientsAvecDossier.Contains(p.Id))
                .OrderBy(p => p.Nom)
                .ToListAsync();

            return View();
        }

        // ==============================
        // CRÉER DOSSIER MÉDICAL (POST) - SECRÉTAIRE
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            // Récupérer les valeurs directement depuis le formulaire
            var patientIdStr = Request.Form["PatientId"].ToString();
            var remarques = Request.Form["Remarques"].ToString();

            // Valider PatientId
            int patientId = 0;
            if (string.IsNullOrEmpty(patientIdStr) || !int.TryParse(patientIdStr, out patientId) || patientId <= 0)
            {
                ModelState.AddModelError("PatientId", "Veuillez sélectionner un patient.");
                patientId = 0; // S'assurer que c'est 0 si invalide
            }

            // Initialiser Remarques si null ou vide (la base de données exige non-null)
            if (string.IsNullOrEmpty(remarques))
            {
                remarques = "";
            }

            // Vérifier si le patient a déjà un dossier médical (seulement si PatientId est valide)
            if (patientId > 0)
            {
                var dossierExistant = await _context.DossierMedicals
                    .FirstOrDefaultAsync(d => d.PatientId == patientId);

                if (dossierExistant != null)
                {
                    ModelState.AddModelError("PatientId", "Ce patient a déjà un dossier médical.");
                }
            }

            if (ModelState.IsValid)
            {
                var dossierMedical = new DossierMedical
                {
                    PatientId = patientId,
                    Remarques = remarques,
                    DateCreation = DateTime.Now
                };

                _context.DossierMedicals.Add(dossierMedical);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Dossier médical créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            // Recharger la liste des patients pour la vue en cas d'erreur
            var patientsAvecDossier = await _context.DossierMedicals
                .Select(d => d.PatientId)
                .ToListAsync();

            ViewBag.Patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive && !patientsAvecDossier.Contains(p.Id))
                .OrderBy(p => p.Nom)
                .ToListAsync();

            // Si un patientId était pré-sélectionné, le conserver
            var patientIdFromForm = Request.Form["PatientId"].ToString();
            if (!string.IsNullOrEmpty(patientIdFromForm) && int.TryParse(patientIdFromForm, out int patientIdForView) && patientIdForView > 0)
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == patientIdForView);
                
                if (patient != null)
                {
                    ViewBag.PatientId = patient.Id;
                    ViewBag.PatientName = $"{patient.Nom} {patient.Prenom}";
                }
            }

            // Créer un modèle pour la vue avec les valeurs du formulaire
            var model = new DossierMedical
            {
                PatientId = !string.IsNullOrEmpty(patientIdFromForm) && int.TryParse(patientIdFromForm, out int pid) ? pid : 0,
                Remarques = remarques
            };

            return View(model);
        }

        // ==============================
        // MODIFIER DOSSIER MÉDICAL (GET) - SECRÉTAIRE
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            var dossier = await _context.DossierMedicals.FindAsync(id);
            
            if (dossier == null)
                return NotFound();

            return View(dossier);
        }

        // ==============================
        // MODIFIER DOSSIER MÉDICAL (POST) - SECRÉTAIRE
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DateCreation,Remarques")] DossierMedical dossierMedical)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            if (id != dossierMedical.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dossierMedical);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Dossier médical modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DossierMedicalExists(dossierMedical.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(dossierMedical);
        }

        private bool DossierMedicalExists(int id)
        {
            return _context.DossierMedicals.Any(e => e.Id == id);
        }
    }
}

