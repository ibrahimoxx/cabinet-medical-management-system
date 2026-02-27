using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    // Contrôleur accessible par Secrétaire, Médecin et Patient
    public class RendezVousController : Controller
    {
        private readonly CabinetMedicalContext _context;

        public RendezVousController(CabinetMedicalContext context)
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
        // CONSULTER RENDEZ-VOUS (SECRÉTAIRE - Tous)
        // ==============================
        public async Task<IActionResult> Index(string searchPatient = "", string searchMedecin = "", string statut = "", string date = "")
        {
            var role = HttpContext.Session.GetString("UserRole");
            
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            IQueryable<RendezVous> query = _context.RendezVous
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Medecin)
                    .ThenInclude(m => m.User);

            // Filtre par nom de patient
            if (!string.IsNullOrEmpty(searchPatient))
            {
                query = query.Where(r => 
                    r.Patient.Nom.Contains(searchPatient) || 
                    r.Patient.Prenom.Contains(searchPatient) ||
                    (r.Patient.Nom + " " + r.Patient.Prenom).Contains(searchPatient));
            }

            // Filtre par nom de médecin
            if (!string.IsNullOrEmpty(searchMedecin))
            {
                query = query.Where(r => 
                    r.Medecin.Nom.Contains(searchMedecin) || 
                    r.Medecin.Prenom.Contains(searchMedecin) ||
                    (r.Medecin.Nom + " " + r.Medecin.Prenom).Contains(searchMedecin));
            }

            // Filtre par statut
            if (!string.IsNullOrEmpty(statut))
            {
                query = query.Where(r => r.Statut == statut);
            }

            // Filtre par date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime dateFilter))
            {
                query = query.Where(r => r.DateRdv.Date == dateFilter.Date);
            }

            var rendezVous = await query
                .OrderByDescending(r => r.DateRdv)
                .ThenByDescending(r => r.HeureRdv)
                .ToListAsync();

            // Passer les valeurs de filtres à la vue pour pré-remplir les champs
            ViewBag.SearchPatient = searchPatient;
            ViewBag.SearchMedecin = searchMedecin;
            ViewBag.Statut = statut;
            ViewBag.Date = date;

            return View(rendezVous);
        }

        // ==============================
        // MES RENDEZ-VOUS (MÉDECIN / PATIENT)
        // ==============================
        public async Task<IActionResult> MesRendezVous(string search = "", string statut = "", string date = "")
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("Username");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Account");

            IQueryable<RendezVous> query = _context.RendezVous
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Medecin)
                    .ThenInclude(m => m.User);

            if (role == "Medecin")
            {
                var medecin = await _context.Medecins.FirstOrDefaultAsync(m => m.UserId == user.Id);
                if (medecin != null)
                    query = query.Where(r => r.MedecinId == medecin.Id);

                // Filtre par nom de patient pour médecin
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => 
                        r.Patient.Nom.Contains(search) || 
                        r.Patient.Prenom.Contains(search) ||
                        (r.Patient.Nom + " " + r.Patient.Prenom).Contains(search));
                }
            }
            else if (role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (patient != null)
                    query = query.Where(r => r.PatientId == patient.Id);

                // Filtre par nom de médecin pour patient
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => 
                        r.Medecin.Nom.Contains(search) || 
                        r.Medecin.Prenom.Contains(search) ||
                        (r.Medecin.Nom + " " + r.Medecin.Prenom).Contains(search));
                }
            }

            // Filtre par statut
            if (!string.IsNullOrEmpty(statut))
            {
                query = query.Where(r => r.Statut == statut);
            }

            // Filtre par date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime dateFilter))
            {
                query = query.Where(r => r.DateRdv.Date == dateFilter.Date);
            }

            var rendezVous = await query
                .OrderByDescending(r => r.DateRdv)
                .ThenByDescending(r => r.HeureRdv)
                .ToListAsync();

            // Passer les valeurs de filtres à la vue
            ViewBag.Search = search;
            ViewBag.Statut = statut;
            ViewBag.Date = date;
            ViewBag.Role = role;

            return View(rendezVous);
        }

        // ==============================
        // CRÉER RENDEZ-VOUS (GET) - SECRÉTAIRE / PATIENT
        // ==============================
        public async Task<IActionResult> Create(int? patientId = null)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire" && role != "Patient")
                return RedirectToAction("Login", "Account");

            // Si c'est un patient, on pré-sélectionne son ID
            int? currentPatientId = patientId;
            if (role == "Patient")
            {
                var username = HttpContext.Session.GetString("Username");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (patient != null)
                        currentPatientId = patient.Id;
                }
            }

            ViewBag.PatientId = currentPatientId;
            ViewBag.IsPatient = (role == "Patient");
            ViewBag.Patients = role == "Secretaire" 
                ? await _context.Patients
                    .Include(p => p.User)
                    .Where(p => p.User.IsActive)
                    .OrderBy(p => p.Nom)
                    .ToListAsync()
                : new List<Patient>();

            // Si un patientId est passé en paramètre (retour après création), pré-sélectionner
            if (role == "Secretaire" && patientId.HasValue)
            {
                ViewBag.SelectedPatientId = patientId.Value;
                TempData["InfoMessage"] = "Patient créé avec succès. Veuillez compléter les informations du rendez-vous.";
            }

            ViewBag.Medecins = await _context.Medecins
                .Include(m => m.User)
                .Where(m => m.User.IsActive)
                .OrderBy(m => m.Nom)
                .ToListAsync();

            return View();
        }

        // ==============================
        // CRÉER RENDEZ-VOUS (POST) - SECRÉTAIRE / PATIENT
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int? PatientId, 
            int? MedecinId, 
            DateTime? DateRdv, 
            string HeureRdv,
            string Statut,
            string Motif)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire" && role != "Patient")
                return RedirectToAction("Login", "Account");

            // Validation des champs requis
            if (!PatientId.HasValue || !MedecinId.HasValue || !DateRdv.HasValue || string.IsNullOrEmpty(HeureRdv))
            {
                ModelState.AddModelError("", "Tous les champs sont obligatoires.");
                return await Create();
            }

            // Convertir l'heure string en TimeSpan (format HH:mm)
            TimeSpan heureRdv;
            if (!TimeSpan.TryParse(HeureRdv, out heureRdv))
            {
                // Essayer le format HH:mm:ss si le premier échoue
                var parts = HeureRdv.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
                {
                    heureRdv = new TimeSpan(hours, minutes, 0);
                }
                else
                {
                    ModelState.AddModelError("", "Format d'heure invalide.");
                    return await Create();
                }
            }

            // Si c'est un patient, s'assurer qu'il ne peut créer que pour lui-même
            if (role == "Patient")
            {
                var username = HttpContext.Session.GetString("Username");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (patient != null && PatientId.Value != patient.Id)
                    {
                        ModelState.AddModelError("", "Vous ne pouvez créer un rendez-vous que pour vous-même.");
                        return await Create();
                    }
                }
            }

            // Validation des règles métier
            var validationError = ValidateAppointment(DateRdv.Value, heureRdv, MedecinId.Value);
            if (!string.IsNullOrEmpty(validationError))
            {
                ModelState.AddModelError("", validationError);
                return await Create();
            }

            // Vérifier si le créneau est déjà pris (même médecin, même date, même heure)
            var existingRDV = await _context.RendezVous
                .FirstOrDefaultAsync(r => r.MedecinId == MedecinId.Value
                    && r.DateRdv.Date == DateRdv.Value.Date
                    && r.HeureRdv == heureRdv
                    && r.Statut != "Annule");

            if (existingRDV != null)
            {
                ModelState.AddModelError("", "Ce créneau est déjà réservé. Veuillez choisir un autre horaire.");
                return await Create();
            }

            // Vérifier si le patient a déjà un rendez-vous le même jour avec le même médecin
            var existingRDVPatient = await _context.RendezVous
                .FirstOrDefaultAsync(r => r.PatientId == PatientId.Value
                    && r.MedecinId == MedecinId.Value
                    && r.DateRdv.Date == DateRdv.Value.Date
                    && r.Statut != "Annule");

            if (existingRDVPatient != null)
            {
                ModelState.AddModelError("", "Vous avez déjà un rendez-vous avec ce médecin aujourd'hui. Veuillez choisir un autre jour.");
                return await Create();
            }

            // Créer l'objet RendezVous
            var rendezVous = new RendezVous
            {
                PatientId = PatientId.Value,
                MedecinId = MedecinId.Value,
                DateRdv = DateRdv.Value,
                HeureRdv = heureRdv,
                Statut = string.IsNullOrEmpty(Statut) ? "Planifie" : Statut,
                Motif = Motif
            };

            _context.RendezVous.Add(rendezVous);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Rendez-vous créé avec succès.";
            
            if (role == "Patient")
                return RedirectToAction(nameof(MesRendezVous));
            else
                return RedirectToAction(nameof(Index));
        }

        // ==============================
        // VALIDATION DES RÈGLES MÉTIER
        // ==============================
        private string ValidateAppointment(DateTime date, TimeSpan heure, int medecinId)
        {
            // Vérifier que c'est un jour ouvrable (lundi-vendredi)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return "Les rendez-vous ne peuvent être pris que du lundi au vendredi.";
            }

            // Vérifier que la date n'est pas dans le passé
            if (date.Date < DateTime.Today)
            {
                return "Vous ne pouvez pas prendre de rendez-vous dans le passé.";
            }

            // Vérifier les heures (08:00 - 17:00)
            var heureMinutes = heure.Hours * 60 + heure.Minutes;
            var heureDebut = 8 * 60; // 08:00 = 480 minutes
            var heureFin = 17 * 60;  // 17:00 = 1020 minutes

            if (heureMinutes < heureDebut || heureMinutes >= heureFin)
            {
                return "Les rendez-vous ne peuvent être pris qu'entre 08:00 et 17:00.";
            }

            // Vérifier que l'heure est un créneau valide (multiples de 30 minutes)
            if (heure.Minutes % 30 != 0)
            {
                return "Les rendez-vous doivent être pris par créneaux de 30 minutes (ex: 08:00, 08:30, 09:00...).";
            }

            return string.Empty;
        }

        // ==============================
        // API : RÉCUPÉRER LES CRÉNEAUX DISPONIBLES
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int medecinId, DateTime date)
        {
            if (!IsAuthorized())
                return Json(new { error = "Non autorisé" });

            // Vérifier que c'est un jour ouvrable
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return Json(new { availableSlots = new List<string>() });
            }

            // Générer tous les créneaux de la journée (08:00 à 16:30 par pas de 30 min)
            var allSlots = new List<string>();
            for (int hour = 8; hour < 17; hour++)
            {
                allSlots.Add($"{hour:D2}:00");
                if (hour < 16) // Pas de 16:30 car on termine à 17:00
                    allSlots.Add($"{hour:D2}:30");
            }

            // Récupérer les créneaux déjà pris
            var takenSlots = await _context.RendezVous
                .Where(r => r.MedecinId == medecinId
                    && r.DateRdv.Date == date.Date
                    && r.Statut != "Annule")
                .Select(r => r.HeureRdv)
                .ToListAsync();

            // Filtrer les créneaux disponibles
            var availableSlots = allSlots.Where(slot =>
            {
                var slotTime = TimeSpan.Parse(slot);
                return !takenSlots.Contains(slotTime);
            }).ToList();

            // Si c'est aujourd'hui, filtrer les heures passées
            if (date.Date == DateTime.Today)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                // Ajouter 30 minutes de buffer
                var minTime = currentTime.Add(TimeSpan.FromMinutes(30));
                availableSlots = availableSlots.Where(slot =>
                {
                    var slotTime = TimeSpan.Parse(slot);
                    return slotTime >= minTime;
                }).ToList();
            }

            // Calculer le pourcentage de disponibilité
            var totalSlots = allSlots.Count;
            var availableCount = availableSlots.Count;
            var medecinDisponible = availableCount > 0;

            return Json(new { 
                availableSlots, 
                medecinDisponible,
                tauxOccupation = totalSlots > 0 ? Math.Round((double)(totalSlots - availableCount) / totalSlots * 100, 1) : 0
            });
        }

        // ==============================
        // MODIFIER RENDEZ-VOUS (GET) - SECRÉTAIRE
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            var rendezVous = await _context.RendezVous.FindAsync(id);
            
            if (rendezVous == null)
                return NotFound();

            ViewBag.Patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive)
                .ToListAsync();

            ViewBag.Medecins = await _context.Medecins
                .Include(m => m.User)
                .Where(m => m.User.IsActive)
                .ToListAsync();

            return View(rendezVous);
        }

        // ==============================
        // MODIFIER RENDEZ-VOUS (POST) - SECRÉTAIRE
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,MedecinId,DateRdv,HeureRdv,Statut,Motif")] RendezVous rendezVous)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire")
                return RedirectToAction("Login", "Account");

            if (id != rendezVous.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rendezVous);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Rendez-vous modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RendezVousExists(rendezVous.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive)
                .ToListAsync();

            ViewBag.Medecins = await _context.Medecins
                .Include(m => m.User)
                .Where(m => m.User.IsActive)
                .ToListAsync();

            return View(rendezVous);
        }

        // ==============================
        // ANNULER RENDEZ-VOUS - SECRÉTAIRE / PATIENT
        // ==============================
        public async Task<IActionResult> Annuler(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Secretaire" && role != "Patient")
                return RedirectToAction("Login", "Account");

            var rendezVous = await _context.RendezVous
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Medecin)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (rendezVous == null)
                return NotFound();

            // Vérifier que le patient ne peut annuler que ses propres RDV
            if (role == "Patient")
            {
                var username = HttpContext.Session.GetString("Username");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (patient != null && rendezVous.PatientId != patient.Id)
                    {
                        TempData["ErrorMessage"] = "Vous ne pouvez annuler que vos propres rendez-vous.";
                        return RedirectToAction(nameof(MesRendezVous));
                    }
                }
            }

            rendezVous.Statut = "Annule";
            await _context.SaveChangesAsync();

            // Créer une alerte pour l'autre partie
            await CreateAlerteAnnulation(rendezVous, role);
            
            TempData["SuccessMessage"] = "Rendez-vous annulé avec succès.";
            
            if (role == "Patient")
                return RedirectToAction(nameof(MesRendezVous));
            else
                return RedirectToAction(nameof(Index));
        }

        // ==============================
        // CRÉER ALERTE D'ANNULATION
        // ==============================
        private async Task CreateAlerteAnnulation(RendezVous rdv, string roleQuiAnnule)
        {
            try
            {
                string message = "";

                if (roleQuiAnnule == "Patient")
                {
                    // Le patient a annulé, alerte pour la secrétaire ET le médecin
                    var secretaires = await _context.Secretaires
                        .Include(s => s.User)
                        .Where(s => s.User.IsActive)
                        .ToListAsync();

                    // Alerte pour les secrétaires
                    foreach (var secretaire in secretaires)
                    {
                        message = $"Le patient {rdv.Patient?.Nom} {rdv.Patient?.Prenom} a annulé son rendez-vous du {rdv.DateRdv:dd/MM/yyyy} à {rdv.HeureRdv:hh\\:mm} avec Dr. {rdv.Medecin?.Nom} {rdv.Medecin?.Prenom}.";
                        
                        var alerte = new Alerte
                        {
                            Type = "AnnulationRDV",
                            Message = message,
                            UserId = secretaire.UserId,
                            RendezVousId = rdv.Id,
                            EstLue = false,
                            DateCreation = DateTime.Now
                        };
                        _context.Alertes.Add(alerte);
                    }

                    // Alerte pour le médecin
                    if (rdv.Medecin?.User != null)
                    {
                        message = $"Le patient {rdv.Patient?.Nom} {rdv.Patient?.Prenom} a annulé son rendez-vous du {rdv.DateRdv:dd/MM/yyyy} à {rdv.HeureRdv:hh\\:mm}.";
                        
                        var alerteMedecin = new Alerte
                        {
                            Type = "AnnulationRDV",
                            Message = message,
                            UserId = rdv.Medecin.UserId,
                            RendezVousId = rdv.Id,
                            EstLue = false,
                            DateCreation = DateTime.Now
                        };
                        _context.Alertes.Add(alerteMedecin);
                    }
                }
                else if (roleQuiAnnule == "Secretaire")
                {
                    // La secrétaire a annulé, alerte pour le patient ET le médecin
                    
                    // Alerte pour le patient
                    if (rdv.Patient?.User != null)
                    {
                        message = $"Votre rendez-vous du {rdv.DateRdv:dd/MM/yyyy} à {rdv.HeureRdv:hh\\:mm} avec Dr. {rdv.Medecin?.Nom} {rdv.Medecin?.Prenom} a été annulé par le secrétariat.";
                        
                        var alertePatient = new Alerte
                        {
                            Type = "AnnulationRDV",
                            Message = message,
                            UserId = rdv.Patient.UserId,
                            RendezVousId = rdv.Id,
                            EstLue = false,
                            DateCreation = DateTime.Now
                        };
                        _context.Alertes.Add(alertePatient);
                    }

                    // Alerte pour le médecin
                    if (rdv.Medecin?.User != null)
                    {
                        message = $"Le rendez-vous du {rdv.DateRdv:dd/MM/yyyy} à {rdv.HeureRdv:hh\\:mm} avec le patient {rdv.Patient?.Nom} {rdv.Patient?.Prenom} a été annulé par le secrétariat.";
                        
                        var alerteMedecin = new Alerte
                        {
                            Type = "AnnulationRDV",
                            Message = message,
                            UserId = rdv.Medecin.UserId,
                            RendezVousId = rdv.Id,
                            EstLue = false,
                            DateCreation = DateTime.Now
                        };
                        _context.Alertes.Add(alerteMedecin);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Logger l'erreur si nécessaire, mais ne pas bloquer l'annulation
                // On peut ajouter un log ici si nécessaire
            }
        }

        // ==============================
        // DÉTAILS RENDEZ-VOUS
        // ==============================
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthorized())
                return RedirectToAction("Login", "Account");

            var rendezVous = await _context.RendezVous
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Medecin)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rendezVous == null)
                return NotFound();

            return View(rendezVous);
        }

        // ==============================
        // API : VÉRIFIER LES RAPPELS (24H AVANT)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> CheckRappels()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return Json(new { rappels = new List<object>() });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Json(new { rappels = new List<object>() });

            var rappels = new List<object>();
            var maintenant = DateTime.Now;
            var dans24h = maintenant.AddHours(24);

            // Récupérer les RDV du patient dans les 24 prochaines heures
            Patient patient = null;
            if (user.Role == "Patient")
            {
                patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            }

            if (patient != null)
            {
                var rdvProches = await _context.RendezVous
                    .Include(r => r.Medecin)
                    .Where(r => r.PatientId == patient.Id
                        && r.Statut == "Planifie"
                        && r.DateRdv.Date == dans24h.Date
                        && r.DateRdv >= maintenant
                        && r.DateRdv <= dans24h)
                    .ToListAsync();

                foreach (var rdv in rdvProches)
                {
                    // Vérifier si une alerte de rappel existe déjà
                    var alerteExistante = await _context.Alertes
                        .FirstOrDefaultAsync(a => a.UserId == user.Id
                            && a.RendezVousId == rdv.Id
                            && a.Type == "RappelRDV");

                    if (alerteExistante == null)
                    {
                        // Créer l'alerte de rappel
                        var alerte = new Alerte
                        {
                            Type = "RappelRDV",
                            Message = $"Rappel : Vous avez un rendez-vous demain le {rdv.DateRdv:dd/MM/yyyy} à {rdv.HeureRdv:hh\\:mm} avec Dr. {rdv.Medecin?.Nom} {rdv.Medecin?.Prenom}.",
                            UserId = user.Id,
                            RendezVousId = rdv.Id,
                            EstLue = false,
                            DateCreation = DateTime.Now
                        };
                        _context.Alertes.Add(alerte);
                        await _context.SaveChangesAsync();

                        rappels.Add(new
                        {
                            message = alerte.Message,
                            date = rdv.DateRdv.ToString("dd/MM/yyyy"),
                            heure = rdv.HeureRdv.ToString(@"hh\:mm")
                        });
                    }
                }
            }

            return Json(new { rappels });
        }

        private bool RendezVousExists(int id)
        {
            return _context.RendezVous.Any(e => e.Id == id);
        }
    }
}

