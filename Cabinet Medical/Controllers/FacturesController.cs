using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class FacturesController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public FacturesController(CabinetMedicalContext context)
        {
            _context = context;
        }

        // Vérification du rôle
        private bool IsSecretaire()
        {
            return HttpContext.Session.GetString("UserRole") == "Secretaire";
        }

        // ==============================
        // CONSULTER FACTURES - SECRÉTAIRE
        // ==============================
        public async Task<IActionResult> Index()
        {
            if (!IsSecretaire())
                return RedirectToAction("Login", "Account");

            var factures = await _context.Factures
                .Include(f => f.Patient)
                    .ThenInclude(p => p.User)
                .Include(f => f.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();

            return View(factures);
        }

        // ==============================
        // MES FACTURES - PATIENT
        // ==============================
        public async Task<IActionResult> MesFactures()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient")
                return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            var factures = await _context.Factures
                .Include(f => f.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(f => f.Paiements)
                .Where(f => f.PatientId == patient.Id)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();

            return View(factures);
        }

        // ==============================
        // CRÉER FACTURE (GET) - SECRÉTAIRE
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsSecretaire())
                return RedirectToAction("Login", "Account");

            ViewBag.Patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive)
                .OrderBy(p => p.Nom)
                .ToListAsync();

            ViewBag.Consultations = await _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            return View();
        }

        // ==============================
        // CRÉER FACTURE (POST) - SECRÉTAIRE
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            if (!IsSecretaire())
                return RedirectToAction("Login", "Account");

            // Récupérer les valeurs depuis le formulaire
            var patientIdStr = Request.Form["PatientId"].ToString();
            var consultationIdStr = Request.Form["ConsultationId"].ToString();
            var montantStr = Request.Form["Montant"].ToString();
            var statut = Request.Form["Statut"].ToString();

            // Déclarer et initialiser les variables
            int patientId = 0;
            int consultationId = 0;
            decimal montant = 0;

            // Valider les champs requis
            if (string.IsNullOrEmpty(patientIdStr) || !int.TryParse(patientIdStr, out patientId) || patientId <= 0)
            {
                ModelState.AddModelError("PatientId", "Veuillez sélectionner un patient.");
            }

            if (string.IsNullOrEmpty(consultationIdStr) || !int.TryParse(consultationIdStr, out consultationId) || consultationId <= 0)
            {
                ModelState.AddModelError("ConsultationId", "Veuillez sélectionner une consultation.");
            }

            if (string.IsNullOrEmpty(montantStr) || !decimal.TryParse(montantStr, out montant) || montant <= 0)
            {
                ModelState.AddModelError("Montant", "Veuillez saisir un montant valide et supérieur à 0.");
            }

            // Initialiser Statut si null ou vide
            if (string.IsNullOrEmpty(statut))
            {
                statut = "NonPayee";
            }

            if (ModelState.IsValid)
            {
                // Vérifier que la consultation appartient bien au patient sélectionné
                var consultation = await _context.Consultations
                    .Include(c => c.DossierMedical)
                    .FirstOrDefaultAsync(c => c.Id == consultationId);

                if (consultation == null)
                {
                    ModelState.AddModelError("ConsultationId", "La consultation sélectionnée n'existe pas.");
                }
                else if (consultation.DossierMedical?.PatientId != patientId)
                {
                    ModelState.AddModelError("ConsultationId", "La consultation sélectionnée n'appartient pas au patient choisi.");
                }
                else
                {
                    var facture = new Facture
                    {
                        PatientId = patientId,
                        ConsultationId = consultationId,
                        Montant = montant,
                        Statut = statut,
                        DateFacture = DateTime.Now
                    };

                    _context.Factures.Add(facture);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Facture créée avec succès.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Recharger les données pour la vue en cas d'erreur
            ViewBag.Patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive)
                .OrderBy(p => p.Nom)
                .ToListAsync();

            ViewBag.Consultations = await _context.Consultations
                .Include(c => c.DossierMedical)
                    .ThenInclude(d => d.Patient)
                .Include(c => c.Medecin)
                    .ThenInclude(m => m.User)
                .OrderByDescending(c => c.DateConsultation)
                .ToListAsync();

            // Créer un modèle pour la vue avec les valeurs du formulaire
            var model = new Facture
            {
                PatientId = patientId > 0 ? patientId : 0,
                ConsultationId = consultationId > 0 ? consultationId : 0,
                Montant = montant,
                Statut = statut
            };

            return View(model);
        }

        // ==============================
        // PAYER FACTURE (GET) - PATIENT
        // ==============================
        public async Task<IActionResult> Payer(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient")
                return RedirectToAction("Login", "Account");

            var facture = await _context.Factures
                .Include(f => f.Patient)
                    .ThenInclude(p => p.User)
                .Include(f => f.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facture == null)
                return NotFound();

            // Vérifier que c'est bien la facture du patient connecté
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (facture.PatientId != patient.Id)
                return Unauthorized();

            return View(facture);
        }

        // ==============================
        // PAYER FACTURE (POST) - PATIENT
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payer(int factureId, string modePaiement)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient")
                return RedirectToAction("Login", "Account");

            var facture = await _context.Factures
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.Id == factureId);

            if (facture == null)
                return NotFound();

            // Vérifier que c'est bien la facture du patient connecté
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (facture.PatientId != patient.Id)
                return Unauthorized();

            // Calculer le montant restant
            decimal montantDejaPaye = facture.Paiements?.Sum(p => p.Montant) ?? 0;
            decimal montantRestant = facture.Montant - montantDejaPaye;

            if (montantRestant <= 0)
            {
                TempData["ErrorMessage"] = "Cette facture est déjà payée.";
                return RedirectToAction("MesFactures");
            }

            // Créer le paiement
            var paiement = new Paiement
            {
                FactureId = factureId,
                ModePaiement = modePaiement, // "EnLigne" ou "Espece"
                Montant = montantRestant,
                DatePaiement = DateTime.Now
            };

            _context.Paiements.Add(paiement);

            // Mettre à jour le statut de la facture si complètement payée
            if (montantRestant >= facture.Montant)
            {
                facture.Statut = "Payee";
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Paiement de {montantRestant:C} effectué avec succès ({modePaiement}).";
            return RedirectToAction("MesFactures");
        }

        // ==============================
        // IMPRIMER FACTURE <<extend>>
        // ==============================
        public async Task<IActionResult> Imprimer(int id)
        {
            var facture = await _context.Factures
                .Include(f => f.Patient)
                    .ThenInclude(p => p.User)
                .Include(f => f.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facture == null)
                return NotFound();

            return View(facture);
        }

        // ==============================
        // DÉTAILS FACTURE
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            var facture = await _context.Factures
                .Include(f => f.Patient)
                    .ThenInclude(p => p.User)
                .Include(f => f.Consultation)
                    .ThenInclude(c => c.Medecin)
                        .ThenInclude(m => m.User)
                .Include(f => f.Paiements)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facture == null)
                return NotFound();

            return View(facture);
        }
    }
}

