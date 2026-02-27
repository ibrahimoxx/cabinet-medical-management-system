using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class ConsultationsController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public ConsultationsController(CabinetMedicalContext context)
        {
            _context = context;
        }

        // Vérification du rôle
        private bool IsMedecin()
        {
            return HttpContext.Session.GetString("UserRole") == "Medecin";
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Medecin" || role == "Secretaire";
        }

        // ==============================
        // CONSULTER CONSULTATIONS
        // ==============================
        public async Task<IActionResult> Index(string searchPatient = "", string date = "")
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            IQueryable<Consultation> query = _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                        .ThenInclude(p => p.User)
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User);

            if (role == "Medecin" && user != null)
            {
                var medecin = await _context.Medecins.FirstOrDefaultAsync(m => m.UserId == user.Id);
                if (medecin != null)
                    query = query.Where(c => c.MedecinId == medecin.Id);
            }

            // Filtre par nom de patient
            if (!string.IsNullOrEmpty(searchPatient))
            {
                query = query.Where(c => 
                    c.DossierMedical.Patient.Nom.Contains(searchPatient) || 
                    c.DossierMedical.Patient.Prenom.Contains(searchPatient) ||
                    (c.DossierMedical.Patient.Nom + " " + c.DossierMedical.Patient.Prenom).Contains(searchPatient));
            }

            // Filtre par date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime dateFilter))
            {
                query = query.Where(c => c.DateConsultation.Date == dateFilter.Date);
            }

            var consultations = await query
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            // Passer les valeurs de filtres à la vue
            ViewBag.SearchPatient = searchPatient;
            ViewBag.Date = date;

            return View(consultations);
        }

        // ==============================
        // CRÉER CONSULTATION (GET) - MÉDECIN
        // ==============================
        public async Task<IActionResult> Create(int? dossierMedicalId)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var medecin = await _context.Medecins.FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (medecin == null)
                return RedirectToAction("Login", "Account");

            ViewBag.MedecinId = medecin.Id;
            ViewBag.DossierMedicals = await _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();

            if (dossierMedicalId.HasValue)
            {
                var consultation = new Consultation
                {
                    DossierMedicalId = dossierMedicalId.Value,
                    MedecinId = medecin.Id,
                    DateConsultation = DateTime.Now
                };
                return View(consultation);
            }

            // Retourner un modèle vide pour éviter NullReferenceException
            var emptyConsultation = new Consultation
            {
                MedecinId = medecin.Id,
                DateConsultation = DateTime.Now
            };
            return View(emptyConsultation);
        }

        // ==============================
        // CRÉER CONSULTATION (POST) - MÉDECIN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            // Récupérer les valeurs depuis le formulaire
            var dossierMedicalIdStr = Request.Form["DossierMedicalId"].ToString();
            var medecinIdStr = Request.Form["MedecinId"].ToString();
            var dateConsultationStr = Request.Form["DateConsultation"].ToString();
            var diagnostic = Request.Form["Diagnostic"].ToString();
            var notes = Request.Form["Notes"].ToString();

            // Déclarer et initialiser les variables
            int dossierMedicalId = 0;
            int medecinId = 0;
            DateTime dateConsultation = DateTime.Now;

            // Valider les champs requis
            if (string.IsNullOrEmpty(dossierMedicalIdStr) || !int.TryParse(dossierMedicalIdStr, out dossierMedicalId) || dossierMedicalId <= 0)
            {
                ModelState.AddModelError("DossierMedicalId", "Veuillez sélectionner un dossier médical.");
            }

            if (string.IsNullOrEmpty(medecinIdStr) || !int.TryParse(medecinIdStr, out medecinId) || medecinId <= 0)
            {
                ModelState.AddModelError("MedecinId", "Médecin invalide.");
            }

            if (string.IsNullOrEmpty(dateConsultationStr) || !DateTime.TryParse(dateConsultationStr, out dateConsultation))
            {
                ModelState.AddModelError("DateConsultation", "Veuillez sélectionner une date de consultation valide.");
            }

            // Initialiser Diagnostic et Notes si null
            if (string.IsNullOrEmpty(diagnostic))
            {
                diagnostic = "";
            }

            if (string.IsNullOrEmpty(notes))
            {
                notes = "";
            }

            if (ModelState.IsValid)
            {
                var consultation = new Consultation
                {
                    DossierMedicalId = dossierMedicalId,
                    MedecinId = medecinId,
                    DateConsultation = dateConsultation,
                    Diagnostic = diagnostic,
                    Notes = notes
                };

                _context.Consultations.Add(consultation);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Consultation créée avec succès.";
                return RedirectToAction(nameof(Index));
            }

            // Recharger les données pour la vue en cas d'erreur
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var medecin = await _context.Medecins.FirstOrDefaultAsync(m => m.UserId == user.Id);

            ViewBag.MedecinId = medecin?.Id;
            ViewBag.DossierMedicals = await _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .OrderByDescending(d => d.DateCreation)
                .ToListAsync();

            // Créer un modèle pour la vue avec les valeurs du formulaire
            var model = new Consultation
            {
                DossierMedicalId = dossierMedicalId > 0 ? dossierMedicalId : 0,
                MedecinId = medecinId > 0 ? medecinId : (medecin?.Id ?? 0),
                DateConsultation = dateConsultation != default ? dateConsultation : DateTime.Now,
                Diagnostic = diagnostic,
                Notes = notes
            };

            return View(model);
        }

        // ==============================
        // MODIFIER CONSULTATION (GET) - MÉDECIN
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var consultation = await _context.Consultations
                .Include(c => c.DossierMedical)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            ViewBag.DossierMedicals = await _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .ToListAsync();

            return View(consultation);
        }

        // ==============================
        // MODIFIER CONSULTATION (POST) - MÉDECIN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DossierMedicalId,MedecinId,DateConsultation,Diagnostic,Notes")] Consultation consultation)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            if (id != consultation.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(consultation);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Consultation modifiée avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultationExists(consultation.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.DossierMedicals = await _context.DossierMedicals
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .ToListAsync();

            return View(consultation);
        }

        // ==============================
        // SUPPRIMER CONSULTATION - MÉDECIN
        // ==============================
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var consultation = await _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                        .ThenInclude(p => p.User)
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            return View(consultation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var consultation = await _context.Consultations.FindAsync(id);
            
            if (consultation != null)
            {
                // 1. Supprimer les paiements des factures liées
                var factures = await _context.Factures
                    .Where(f => f.ConsultationId == consultation.Id)
                    .Include(f => f.Paiements)
                    .ToListAsync();

                foreach (var facture in factures)
                {
                    if (facture.Paiements != null && facture.Paiements.Any())
                    {
                        _context.Paiements.RemoveRange(facture.Paiements);
                    }
                }

                // 2. Supprimer les factures associées
                _context.Factures.RemoveRange(factures);

                // 3. Supprimer les ordonnances associées (avec leurs détails)
                var ordonnances = await _context.Ordonnances
                    .Where(o => o.ConsultationId == consultation.Id)
                    .ToListAsync();

                foreach (var ordonnance in ordonnances)
                {
                    var details = await _context.OrdonnanceDetails
                        .Where(od => od.OrdonnanceId == ordonnance.Id)
                        .ToListAsync();
                    _context.OrdonnanceDetails.RemoveRange(details);
                }
                _context.Ordonnances.RemoveRange(ordonnances);

                // 4. Supprimer la consultation
                _context.Consultations.Remove(consultation);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Consultation supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // DÉTAILS CONSULTATION
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var consultation = await _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                        .ThenInclude(p => p.User)
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            // Charger les ordonnances
            var ordonnances = await _context.Ordonnances
                .Include(o => o.OrdonnanceDetails)
                .Where(o => o.ConsultationId == consultation.Id)
                .ToListAsync();

            ViewBag.Ordonnances = ordonnances;

            return View(consultation);
        }

        // ==============================
        // IMPRIMER CONSULTATION <<extend>>
        // ==============================
        public async Task<IActionResult> Imprimer(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var consultation = await _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                        .ThenInclude(p => p.User)
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            return View(consultation);
        }

        private bool ConsultationExists(int id)
        {
            return _context.Consultations.Any(e => e.Id == id);
        }
    }
}

