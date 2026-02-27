using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Cabinet_Medical.Controllers
{
    public class OrdonnancesController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public OrdonnancesController(CabinetMedicalContext context)
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
            return role == "Medecin" || role == "Patient" || role == "Secretaire";
        }

        // ==============================
        // CONSULTER ORDONNANCES - MÉDECIN
        // ==============================
        public async Task<IActionResult> Index(string searchPatient = "", string date = "")
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var medecin = await _context.Medecins.FirstOrDefaultAsync(m => m.UserId == user.Id);

            IQueryable<Ordonnance> query = _context.Ordonnances
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.DossierMedical)
                        .ThenInclude(d => d.Patient)
                            .ThenInclude(p => p.User)
                .Include(o => o.OrdonnanceDetails)
                .Where(o => o.Consultation.MedecinId == medecin.Id);

            // Filtre par nom de patient
            if (!string.IsNullOrEmpty(searchPatient))
            {
                query = query.Where(o => 
                    o.Consultation.DossierMedical.Patient.Nom.Contains(searchPatient) || 
                    o.Consultation.DossierMedical.Patient.Prenom.Contains(searchPatient) ||
                    (o.Consultation.DossierMedical.Patient.Nom + " " + o.Consultation.DossierMedical.Patient.Prenom).Contains(searchPatient));
            }

            // Filtre par date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime dateFilter))
            {
                query = query.Where(o => o.DateOrdonnance.Date == dateFilter.Date);
            }

            var ordonnances = await query
                .OrderByDescending(o => o.DateOrdonnance)
                .ToListAsync();

            // Passer les valeurs de filtres à la vue
            ViewBag.SearchPatient = searchPatient;
            ViewBag.Date = date;

            return View(ordonnances);
        }

        // ==============================
        // MES ORDONNANCES - PATIENT
        // ==============================
        public async Task<IActionResult> MesOrdonnances()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient")
                return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var dossier = await _context.DossierMedicals
                .FirstOrDefaultAsync(d => d.PatientId == patient.Id);

            if (dossier == null)
            {
                TempData["InfoMessage"] = "Aucune ordonnance trouvée.";
                return View(new List<Ordonnance>());
            }

            var ordonnances = await _context.Ordonnances
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(o => o.OrdonnanceDetails)
                .Where(o => o.Consultation.DossierMedicalId == dossier.Id)
                .OrderByDescending(o => o.DateOrdonnance)
                .ToListAsync();

            return View(ordonnances);
        }

        // ==============================
        // CRÉER ORDONNANCE (GET) - MÉDECIN
        // ==============================
        public async Task<IActionResult> Create(int consultationId)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var consultation = await _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == consultationId);

            if (consultation == null)
                return NotFound();

            ViewBag.Consultation = consultation;

            var ordonnance = new Ordonnance
            {
                ConsultationId = consultationId,
                DateOrdonnance = DateTime.Now
            };

            return View(ordonnance);
        }

        // ==============================
        // CRÉER ORDONNANCE (POST) - MÉDECIN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int consultationId, string[] types, string[] descriptions, string[] dosages)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            if (types == null || types.Length == 0)
            {
                ModelState.AddModelError("", "Au moins un élément doit être ajouté à l'ordonnance.");
                
                var consultation = await _context.Consultations
                    .Include(c => c.DossierMedical)
                        .ThenInclude(d => d.Patient)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(c => c.Id == consultationId);

                ViewBag.Consultation = consultation;
                return View(new Ordonnance { ConsultationId = consultationId, DateOrdonnance = DateTime.Now });
            }

            var ordonnance = new Ordonnance
            {
                ConsultationId = consultationId,
                DateOrdonnance = DateTime.Now
            };

            _context.Ordonnances.Add(ordonnance);
            await _context.SaveChangesAsync();

            // Ajouter les détails
            for (int i = 0; i < types.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(types[i]))
                {
                    var detail = new OrdonnanceDetail
                    {
                        OrdonnanceId = ordonnance.Id,
                        Type = types[i],
                        Description = descriptions != null && i < descriptions.Length ? descriptions[i] : "",
                        Dosage = dosages != null && i < dosages.Length ? dosages[i] : ""
                    };
                    _context.OrdonnanceDetails.Add(detail);
                }
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Ordonnance créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // MODIFIER ORDONNANCE (GET) - MÉDECIN
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var ordonnance = await _context.Ordonnances
                .Include(o => o.Consultation)
                .Include(o => o.OrdonnanceDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordonnance == null)
                return NotFound();

            return View(ordonnance);
        }

        // ==============================
        // MODIFIER ORDONNANCE (POST) - MÉDECIN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int[] detailIds, string[] types, string[] descriptions, string[] dosages)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var ordonnance = await _context.Ordonnances
                .Include(o => o.OrdonnanceDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordonnance == null)
            {
                TempData["ErrorMessage"] = "Ordonnance introuvable.";
                return RedirectToAction(nameof(Index));
            }

            // Validation : au moins un élément doit être présent
            if (types == null || types.Length == 0 || types.All(t => string.IsNullOrWhiteSpace(t)))
            {
                ModelState.AddModelError("", "Au moins un élément doit être présent dans l'ordonnance.");
                
                // Recharger l'ordonnance pour la vue
                ordonnance = await _context.Ordonnances
                    .Include(o => o.Consultation)
                    .Include(o => o.OrdonnanceDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);
                    
                return View(ordonnance);
            }

            try
            {
                // Supprimer les détails existants
                if (ordonnance.OrdonnanceDetails != null && ordonnance.OrdonnanceDetails.Any())
                {
                    _context.OrdonnanceDetails.RemoveRange(ordonnance.OrdonnanceDetails);
                }

                // Ajouter les nouveaux détails
                for (int i = 0; i < types.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(types[i]))
                    {
                        var description = descriptions != null && i < descriptions.Length ? descriptions[i] : "";
                        var dosage = dosages != null && i < dosages.Length ? dosages[i] : "";
                        
                        // Validation : description est requise
                        if (string.IsNullOrWhiteSpace(description))
                        {
                            ModelState.AddModelError("", $"La description est requise pour l'élément {i + 1}.");
                            
                            // Recharger l'ordonnance pour la vue
                            ordonnance = await _context.Ordonnances
                                .Include(o => o.Consultation)
                                .Include(o => o.OrdonnanceDetails)
                                .FirstOrDefaultAsync(o => o.Id == id);
                                
                            return View(ordonnance);
                        }
                        
                        var detail = new OrdonnanceDetail
                        {
                            OrdonnanceId = ordonnance.Id,
                            Type = types[i].Trim(),
                            Description = description.Trim(),
                            Dosage = string.IsNullOrWhiteSpace(dosage) ? null : dosage.Trim()
                        };
                        _context.OrdonnanceDetails.Add(detail);
                    }
                }

                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Ordonnance modifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Une erreur est survenue lors de la modification de l'ordonnance.");
                
                // Recharger l'ordonnance pour la vue
                ordonnance = await _context.Ordonnances
                    .Include(o => o.Consultation)
                    .Include(o => o.OrdonnanceDetails)
                    .FirstOrDefaultAsync(o => o.Id == id);
                    
                return View(ordonnance);
            }
        }

        // ==============================
        // SUPPRIMER ORDONNANCE - MÉDECIN
        // ==============================
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var ordonnance = await _context.Ordonnances
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.DossierMedical)
                        .ThenInclude(d => d.Patient)
                            .ThenInclude(p => p.User)
                .Include(o => o.OrdonnanceDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordonnance == null)
                return NotFound();

            return View(ordonnance);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsMedecin())
                return RedirectToAction("Login", "Account");

            var ordonnance = await _context.Ordonnances
                .Include(o => o.OrdonnanceDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (ordonnance != null)
            {
                _context.OrdonnanceDetails.RemoveRange(ordonnance.OrdonnanceDetails);
                _context.Ordonnances.Remove(ordonnance);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Ordonnance supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // DÉTAILS ORDONNANCE
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var ordonnance = await _context.Ordonnances
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.DossierMedical)
                        .ThenInclude(d => d.Patient)
                            .ThenInclude(p => p.User)
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(o => o.OrdonnanceDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordonnance == null)
                return NotFound();

            return View(ordonnance);
        }

        // ==============================
        // IMPRIMER ORDONNANCE <<extend>>
        // ==============================
        public async Task<IActionResult> Imprimer(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var ordonnance = await _context.Ordonnances
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.DossierMedical)
                        .ThenInclude(d => d.Patient)
                            .ThenInclude(p => p.User)
                .Include(o => o.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(o => o.OrdonnanceDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordonnance == null)
                return NotFound();

            return View(ordonnance);
        }
    }
}

