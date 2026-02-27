using Cabinet_Medical.Controllers.Base;
using Cabinet_Medical.Data;
using Cabinet_Medical.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinet_Medical.Controllers
{
    public class UsersController : RoleController
    {
        private readonly CabinetMedicalContext _context;

        public UsersController(CabinetMedicalContext context)
            : base("Admin")
        {
            _context = context;
        }

        // ==============================
        // CONSULTER UTILISATEURS
        // ==============================
        public async Task<IActionResult> Index(string search = "", string role = "", string statut = "")
        {
            IQueryable<User> query = _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Medecin)
                .Include(u => u.Secretaire);

            // Filtre par recherche (username, email)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.Username.Contains(search) || 
                    u.Email.Contains(search));
            }

            // Filtre par rôle
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            // Filtre par statut
            if (!string.IsNullOrEmpty(statut))
            {
                bool isActive = statut == "Actif";
                query = query.Where(u => u.IsActive == isActive);
            }

            var users = await query
                .OrderBy(u => u.Username)
                .ToListAsync();

            // Passer les valeurs de filtres à la vue
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Statut = statut;

            return View(users);
        }

        // ==============================
        // AJOUTER UTILISATEUR (GET)
        // ==============================
        public IActionResult Create()
        {
            return View();
        }

        // ==============================
        // AJOUTER UTILISATEUR (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,PasswordHash,Email,Role,IsActive")] User user)
        {
            // Supprimer les erreurs de validation pour les propriétés de navigation
            ModelState.Remove("Patient");
            ModelState.Remove("Medecin");
            ModelState.Remove("Secretaire");

            // Validation selon le rôle sélectionné
            if (user.Role == "Patient")
            {
                string patientNom = Request.Form["PatientNom"].ToString().Trim();
                string patientPrenom = Request.Form["PatientPrenom"].ToString().Trim();
                string patientAdresse = Request.Form["PatientAdresse"].ToString().Trim();
                string patientTelephone = Request.Form["PatientTelephone"].ToString().Trim();
                string patientAntecedents = Request.Form["PatientAntecedents"].ToString().Trim();

                if (string.IsNullOrEmpty(patientNom))
                    ModelState.AddModelError("", "Le nom du patient est requis.");
                if (string.IsNullOrEmpty(patientPrenom))
                    ModelState.AddModelError("", "Le prénom du patient est requis.");
                if (string.IsNullOrEmpty(patientAdresse))
                    ModelState.AddModelError("", "L'adresse du patient est requise.");
                if (string.IsNullOrEmpty(patientTelephone))
                    ModelState.AddModelError("", "Le téléphone du patient est requis.");
                if (string.IsNullOrEmpty(patientAntecedents))
                    ModelState.AddModelError("", "Les antécédents médicaux du patient sont requis.");
            }
            else if (user.Role == "Medecin")
            {
                string medecinNom = Request.Form["MedecinNom"].ToString().Trim();
                string medecinPrenom = Request.Form["MedecinPrenom"].ToString().Trim();
                string medecinSpecialite = Request.Form["MedecinSpecialite"].ToString().Trim();
                string medecinTelephone = Request.Form["MedecinTelephone"].ToString().Trim();

                if (string.IsNullOrEmpty(medecinNom))
                    ModelState.AddModelError("", "Le nom du médecin est requis.");
                if (string.IsNullOrEmpty(medecinPrenom))
                    ModelState.AddModelError("", "Le prénom du médecin est requis.");
                if (string.IsNullOrEmpty(medecinSpecialite))
                    ModelState.AddModelError("", "La spécialité du médecin est requise.");
                if (string.IsNullOrEmpty(medecinTelephone))
                    ModelState.AddModelError("", "Le téléphone du médecin est requis.");
            }
            else if (user.Role == "Secretaire")
            {
                string secretaireNom = Request.Form["SecretaireNom"].ToString().Trim();
                string secretairePrenom = Request.Form["SecretairePrenom"].ToString().Trim();
                string secretaireTelephone = Request.Form["SecretaireTelephone"].ToString().Trim();

                if (string.IsNullOrEmpty(secretaireNom))
                    ModelState.AddModelError("", "Le nom du secrétaire est requis.");
                if (string.IsNullOrEmpty(secretairePrenom))
                    ModelState.AddModelError("", "Le prénom du secrétaire est requis.");
                if (string.IsNullOrEmpty(secretaireTelephone))
                    ModelState.AddModelError("", "Le téléphone du secrétaire est requis.");
            }

            // Vérifier si le username existe déjà
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Ce nom d'utilisateur existe déjà.");
            }

            // Vérifier si l'email existe déjà
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
            }

            if (!ModelState.IsValid)
                return View(user);

            user.IsActive = true;
            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Créer l'entité associée selon le rôle avec les données du formulaire
            switch (user.Role)
            {
                case "Patient":
                    string patientDateNaissance = Request.Form["PatientDateNaissance"].ToString().Trim();
                    DateTime? dateNaissance = null;
                    if (!string.IsNullOrEmpty(patientDateNaissance) && DateTime.TryParse(patientDateNaissance, out DateTime parsedDate))
                    {
                        dateNaissance = parsedDate;
                    }

                    _context.Patients.Add(new Patient 
                    { 
                        UserId = user.Id,
                        Nom = Request.Form["PatientNom"].ToString().Trim(),
                        Prenom = Request.Form["PatientPrenom"].ToString().Trim(),
                        DateNaissance = dateNaissance,
                        Adresse = Request.Form["PatientAdresse"].ToString().Trim(),
                        Telephone = Request.Form["PatientTelephone"].ToString().Trim(),
                        AntecedentsMedicaux = Request.Form["PatientAntecedents"].ToString().Trim()
                    });
                    break;

                case "Medecin":
                    _context.Medecins.Add(new Medecin 
                    { 
                        UserId = user.Id,
                        Nom = Request.Form["MedecinNom"].ToString().Trim(),
                        Prenom = Request.Form["MedecinPrenom"].ToString().Trim(),
                        Specialite = Request.Form["MedecinSpecialite"].ToString().Trim(),
                        Telephone = Request.Form["MedecinTelephone"].ToString().Trim()
                    });
                    break;

                case "Secretaire":
                    _context.Secretaires.Add(new Secretaire 
                    { 
                        UserId = user.Id,
                        Nom = Request.Form["SecretaireNom"].ToString().Trim(),
                        Prenom = Request.Form["SecretairePrenom"].ToString().Trim(),
                        Telephone = Request.Form["SecretaireTelephone"].ToString().Trim()
                    });
                    break;
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Utilisateur créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // MODIFIER UTILISATEUR (GET)
        // ==============================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // ==============================
        // MODIFIER UTILISATEUR (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Username,Email,Role,IsActive")] User model)
        {
            var user = await _context.Users.FindAsync(model.Id);

            if (user == null)
                return NotFound();

            // Supprimer les erreurs de validation pour les propriétés de navigation
            ModelState.Remove("Patient");
            ModelState.Remove("Medecin");
            ModelState.Remove("Secretaire");
            ModelState.Remove("PasswordHash");

            // Vérifier si l'email existe déjà pour un autre utilisateur
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé par un autre utilisateur.");
            }

            if (!ModelState.IsValid)
            {
                model.Username = user.Username; // Restaurer le username pour l'affichage
                return View(model);
            }

            // 🔒 Sécurité : Admin reste Admin et actif
            if (user.Role == "Admin")
            {
                user.Email = model.Email;
                user.IsActive = true;
            }
            else
            {
                user.Email = model.Email;
                user.Role = model.Role;
                user.IsActive = model.IsActive;
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Utilisateur modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // ACTIVER / DESACTIVER (SAUF ADMIN)
        // ==============================
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            if (user.Role == "Admin")
                return RedirectToAction(nameof(Index));

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // SUPPRIMER UTILISATEUR (SAUF ADMIN)
        // ==============================
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Medecin)
                .Include(u => u.Secretaire)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            if (user.Role == "Admin")
                return RedirectToAction(nameof(Index));

            // Supprimer entités liées selon le rôle
            if (user.Patient != null)
            {
                await DeletePatientDataAsync(user.Patient.Id);
                _context.Patients.Remove(user.Patient);
            }

            if (user.Medecin != null)
            {
                await DeleteMedecinDataAsync(user.Medecin.Id);
                _context.Medecins.Remove(user.Medecin);
            }

            if (user.Secretaire != null)
            {
                _context.Secretaires.Remove(user.Secretaire);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Utilisateur supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ==============================
        // SUPPRIMER DONNÉES PATIENT
        // ==============================
        private async Task DeletePatientDataAsync(int patientId)
        {
            // 1. Supprimer les Paiements des Factures du Patient
            var factures = await _context.Factures
                .Where(f => f.PatientId == patientId)
                .ToListAsync();

            foreach (var facture in factures)
            {
                var paiements = await _context.Paiements
                    .Where(p => p.FactureId == facture.Id)
                    .ToListAsync();
                _context.Paiements.RemoveRange(paiements);
            }

            // 2. Supprimer les Factures du Patient
            _context.Factures.RemoveRange(factures);

            // 3. Supprimer les DossierMedical et leurs données
            var dossiers = await _context.DossierMedicals
                .Where(d => d.PatientId == patientId)
                .ToListAsync();

            foreach (var dossier in dossiers)
            {
                // Supprimer OrdonnanceDetails des Consultations
                var consultations = await _context.Consultations
                    .Where(c => c.DossierMedicalId == dossier.Id)
                    .ToListAsync();

                foreach (var consultation in consultations)
                {
                    var ordonnances = await _context.Ordonnances
                        .Where(o => o.ConsultationId == consultation.Id)
                        .ToListAsync();

                    foreach (var ordonnance in ordonnances)
                    {
                        var ordonnanceDetails = await _context.OrdonnanceDetails
                            .Where(od => od.OrdonnanceId == ordonnance.Id)
                            .ToListAsync();
                        _context.OrdonnanceDetails.RemoveRange(ordonnanceDetails);
                    }

                    _context.Ordonnances.RemoveRange(ordonnances);
                }

                // Supprimer les Consultations
                _context.Consultations.RemoveRange(consultations);
            }

            // Supprimer les DossierMedical
            _context.DossierMedicals.RemoveRange(dossiers);

            // 4. Supprimer les RendezVous du Patient
            var rendezVous = await _context.RendezVous
                .Where(r => r.PatientId == patientId)
                .ToListAsync();
            _context.RendezVous.RemoveRange(rendezVous);

            await _context.SaveChangesAsync();
        }

        // ==============================
        // SUPPRIMER DONNÉES MEDECIN
        // ==============================
        private async Task DeleteMedecinDataAsync(int medecinId)
        {
            // 1. Supprimer les Consultations du Medecin et leurs données
            var consultations = await _context.Consultations
                .Where(c => c.MedecinId == medecinId)
                .ToListAsync();

            foreach (var consultation in consultations)
            {
                // Supprimer Paiements des Factures de cette Consultation
                var factures = await _context.Factures
                    .Where(f => f.ConsultationId == consultation.Id)
                    .ToListAsync();

                foreach (var facture in factures)
                {
                    var paiements = await _context.Paiements
                        .Where(p => p.FactureId == facture.Id)
                        .ToListAsync();
                    _context.Paiements.RemoveRange(paiements);
                }

                // Supprimer les Factures
                _context.Factures.RemoveRange(factures);

                // Supprimer OrdonnanceDetails des Ordonnances
                var ordonnances = await _context.Ordonnances
                    .Where(o => o.ConsultationId == consultation.Id)
                    .ToListAsync();

                foreach (var ordonnance in ordonnances)
                {
                    var ordonnanceDetails = await _context.OrdonnanceDetails
                        .Where(od => od.OrdonnanceId == ordonnance.Id)
                        .ToListAsync();
                    _context.OrdonnanceDetails.RemoveRange(ordonnanceDetails);
                }

                // Supprimer les Ordonnances
                _context.Ordonnances.RemoveRange(ordonnances);
            }

            // Supprimer les Consultations
            _context.Consultations.RemoveRange(consultations);

            // 2. Supprimer les RendezVous du Medecin
            var rendezVous = await _context.RendezVous
                .Where(r => r.MedecinId == medecinId)
                .ToListAsync();
            _context.RendezVous.RemoveRange(rendezVous);

            await _context.SaveChangesAsync();
        }
    }
}
