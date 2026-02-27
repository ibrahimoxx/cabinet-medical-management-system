# 📁 DESCRIPTION DES FICHIERS DU PROJET

Ce document décrit brièvement le rôle et la fonction de chaque fichier important du projet Cabinet Medical.

---

## 📂 FICHIERS RACINE

### `Cabinet Medical.sln`
**Rôle :** Fichier solution Visual Studio  
**Description :** Définit la structure globale du projet, lie tous les projets et configurations.

### `Cabinet Medical.csproj`
**Rôle :** Fichier de projet .NET  
**Description :** Définit le framework cible (net10.0), les packages NuGet (Entity Framework Core, SQL Server), et la configuration du projet.

### `Program.cs`
**Rôle :** Point d'entrée de l'application  
**Description :** Configure les services (MVC, Entity Framework, Sessions), le middleware (HTTPS, routing, static files), et démarre l'application web.

### `appsettings.json`
**Rôle :** Configuration de l'application  
**Description :** Contient la chaîne de connexion à la base de données SQL Server et les paramètres de logging.

### `appsettings.Development.json`
**Rôle :** Configuration spécifique au développement  
**Description :** Paramètres utilisés uniquement en mode développement.

### `Properties/launchSettings.json`
**Rôle :** Configuration de lancement  
**Description :** Définit les ports, URLs, et profils de lancement (IIS Express, Kestrel).

---

## 📂 MODELS (Modèles de données)

### `Models/User.cs`
**Rôle :** Modèle utilisateur  
**Description :** Représente un utilisateur du système (Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt). Base pour tous les rôles (Admin, Medecin, Secretaire, Patient).

### `Models/Patient.cs`
**Rôle :** Modèle patient  
**Description :** Informations du patient (Nom, Prenom, DateNaissance, Adresse, Telephone, AntecedentsMedicaux). Relation 1-1 avec User.

### `Models/Medecin.cs`
**Rôle :** Modèle médecin  
**Description :** Informations du médecin (Specialite, NumeroOrdre). Relation 1-1 avec User.

### `Models/Secretaire.cs`
**Rôle :** Modèle secrétaire  
**Description :** Informations de la secrétaire (DateEmbauche). Relation 1-1 avec User.

### `Models/RendezVous.cs`
**Rôle :** Modèle rendez-vous  
**Description :** Représente un rendez-vous médical (DateRdv, HeureRdv, Statut, Motif). Relations avec Patient et Medecin.

### `Models/DossierMedical.cs`
**Rôle :** Modèle dossier médical  
**Description :** Dossier médical d'un patient (DateCreation, Remarques). Relation 1-1 avec Patient, 1-* avec Consultations.

### `Models/Consultation.cs`
**Rôle :** Modèle consultation  
**Description :** Consultation médicale (DateConsultation, Diagnostic, Notes). Liée à DossierMedical et Medecin.

### `Models/Ordonnance.cs`
**Rôle :** Modèle ordonnance  
**Description :** Ordonnance médicale (DateOrdonnance). Relation 1-1 avec Consultation, 1-* avec OrdonnanceDetails.

### `Models/OrdonnanceDetail.cs`
**Rôle :** Modèle détail d'ordonnance  
**Description :** Détail d'une ordonnance (Type: Medicament/Analyse/Radiologie, Description, Dosage). Relation *-1 avec Ordonnance.

### `Models/Facture.cs`
**Rôle :** Modèle facture  
**Description :** Facture médicale (Montant, Statut: NonPayee/Payee). Relations avec Patient, Consultation (1-1), et Paiements (1-*).

### `Models/Paiement.cs`
**Rôle :** Modèle paiement  
**Description :** Paiement d'une facture (ModePaiement: EnLigne/Espece, Montant, DatePaiement). Relation *-1 avec Facture.

### `Models/Alerte.cs`
**Rôle :** Modèle alerte  
**Description :** Notification système (Type: AnnulationRDV/RappelRDV, Message, EstLue). Relations avec User et RendezVous (optionnel).

### `Models/ErrorViewModel.cs`
**Rôle :** Modèle d'erreur  
**Description :** Modèle pour afficher les erreurs (RequestId, ShowRequestId).

---

## 📂 DATA (Accès aux données)

### `Data/CabinetMedicalContext.cs`
**Rôle :** Contexte Entity Framework  
**Description :** Définit les DbSet pour toutes les entités, configure les relations (1-1, 1-*), et les contraintes de suppression (DeleteBehavior.NoAction) dans OnModelCreating.

---

## 📂 CONTROLLERS (Logique métier)

### `Controllers/AccountController.cs`
**Rôle :** Authentification  
**Description :** Gère Login (vérification credentials, création session), Register (inscription patients), Logout (destruction session).

### `Controllers/DashboardController.cs`
**Rôle :** Routage des dashboards  
**Description :** Redirige vers le dashboard approprié selon le rôle de l'utilisateur (Admin, Secretaire, Medecin, Patient).

### `Controllers/RoleController.cs`
**Rôle :** Classe de base pour contrôleurs protégés  
**Description :** Classe abstraite qui vérifie automatiquement le rôle requis avant chaque action. Redirige vers Login si non authentifié, ou Dashboard si mauvais rôle.

### `Controllers/HomeController.cs`
**Rôle :** Page d'accueil  
**Description :** Affiche la page d'accueil publique et la page Privacy.

### `Controllers/PatientsController.cs`
**Rôle :** Gestion des patients  
**Description :** CRUD patients (Create avec création User automatique, Index avec recherche, Edit, Delete). Intégration avec formulaire RDV (returnUrl).

### `Controllers/UsersController.cs`
**Rôle :** Gestion des utilisateurs (Admin)  
**Description :** CRUD utilisateurs avec filtres (recherche, rôle, statut). Réservé aux administrateurs.

### `Controllers/RendezVousController.cs`
**Rôle :** Gestion des rendez-vous  
**Description :** CRUD rendez-vous, validation créneaux disponibles (GetAvailableSlots), annulation avec alertes, rappels automatiques (CheckRappels), filtres multiples.

### `Controllers/DossierMedicalsController.cs`
**Rôle :** Gestion des dossiers médicaux  
**Description :** CRUD dossiers médicaux, consultation historique complet (Consulter), vue patient (MonDossier), filtres par nom patient.

### `Controllers/ConsultationsController.cs`
**Rôle :** Gestion des consultations  
**Description :** CRUD consultations (réservé médecins), suppression en cascade (Paiements → Factures → Ordonnances → Consultation), impression, filtres.

### `Controllers/OrdonnancesController.cs`
**Rôle :** Gestion des ordonnances  
**Description :** CRUD ordonnances avec détails dynamiques, modification (suppression/recréation détails), impression, vue patient (MesOrdonnances), filtres.

### `Controllers/FacturesController.cs`
**Rôle :** Gestion de la facturation  
**Description :** CRUD factures (Secrétaire), paiements multiples (Patient), calcul automatique statut "Payee", impression, vue patient (MesFactures).

### `Controllers/AlertesController.cs`
**Rôle :** Gestion des alertes  
**Description :** Affichage des alertes non lues, marquer comme lue, API GetUnreadCount pour badge notification.

### `Controllers/ProfileController.cs`
**Rôle :** Profil utilisateur  
**Description :** Affichage et modification du profil utilisateur (tous les rôles).

### `Controllers/AdminDashboardController.cs`
**Rôle :** Dashboard administrateur  
**Description :** Affiche statistiques globales (utilisateurs, patients, médecins actifs).

### `Controllers/SecretaireDashboardController.cs`
**Rôle :** Dashboard secrétaire  
**Description :** Affiche statistiques secrétaire (patients, RDV, factures impayées).

### `Controllers/MedecinDashboardController.cs`
**Rôle :** Dashboard médecin  
**Description :** Affiche statistiques médecin (RDV aujourd'hui, consultations, ordonnances).

### `Controllers/PatientDashboardController.cs`
**Rôle :** Dashboard patient  
**Description :** Affiche statistiques patient (prochains RDV, factures en attente, ordonnances).

---

## 📂 FILTERS (Filtres personnalisés)

### `Filters/RoleAuthorizeAttribute.cs`
**Rôle :** Attribut d'autorisation par rôle  
**Description :** Filtre personnalisé pour vérifier le rôle utilisateur avant d'exécuter une action (alternative à RoleController).

---

## 📂 VIEWS (Interface utilisateur)

### `Views/_ViewStart.cshtml`
**Rôle :** Démarrage des vues  
**Description :** Définit le layout par défaut (_Layout.cshtml) pour toutes les vues.

### `Views/_ViewImports.cshtml`
**Rôle :** Imports globaux  
**Description :** Importe les namespaces nécessaires (Models, TagHelpers) pour toutes les vues Razor.

### `Views/Shared/_Layout.cshtml`
**Rôle :** Layout principal  
**Description :** Structure HTML de base (header, menu navigation selon rôle, footer), badge notification alertes, JavaScript global (protection routes, actualisation alertes).

### `Views/Shared/_ValidationScriptsPartial.cshtml`
**Rôle :** Scripts de validation  
**Description :** Inclut jQuery Validation et Unobtrusive pour la validation côté client.

### `Views/Shared/Error.cshtml`
**Rôle :** Page d'erreur  
**Description :** Affiche les erreurs avec RequestId (en développement).

### `Views/Account/Login.cshtml`
**Rôle :** Page de connexion  
**Description :** Formulaire de connexion (Username/Email, Password) avec validation.

### `Views/Account/Register.cshtml`
**Rôle :** Page d'inscription  
**Description :** Formulaire d'inscription pour patients (création User + Patient).

### `Views/Dashboard/Admin.cshtml`
**Rôle :** Dashboard administrateur  
**Description :** Affiche statistiques globales avec cards Bootstrap (total users, actifs, patients, médecins).

### `Views/Dashboard/Secretaire.cshtml`
**Rôle :** Dashboard secrétaire  
**Description :** Affiche statistiques secrétaire (patients, RDV, RDV aujourd'hui, factures impayées) et liens rapides.

### `Views/Dashboard/Medecin.cshtml`
**Rôle :** Dashboard médecin  
**Description :** Affiche statistiques médecin (RDV aujourd'hui, consultations, ordonnances) et liens rapides.

### `Views/Dashboard/Patient.cshtml`
**Rôle :** Dashboard patient  
**Description :** Affiche statistiques patient (prochains RDV, factures en attente, ordonnances) et liens rapides.

### `Views/Home/Index.cshtml`
**Rôle :** Page d'accueil publique  
**Description :** Page d'accueil du site (accessible sans authentification).

### `Views/Home/Privacy.cshtml`
**Rôle :** Page confidentialité  
**Description :** Page d'informations sur la confidentialité.

### `Views/Patients/Index.cshtml`
**Rôle :** Liste des patients  
**Description :** Tableau des patients avec filtres (nom, prénom, email, téléphone) et actions (Create, Edit, Delete, Details).

### `Views/Patients/Create.cshtml`
**Rôle :** Création patient  
**Description :** Formulaire création patient avec création User automatique (Username, Email, Password, informations patient).

### `Views/Patients/Edit.cshtml`
**Rôle :** Modification patient  
**Description :** Formulaire de modification des informations patient.

### `Views/Patients/Details.cshtml`
**Rôle :** Détails patient  
**Description :** Affichage détaillé des informations d'un patient.

### `Views/Patients/Delete.cshtml`
**Rôle :** Suppression patient  
**Description :** Confirmation de suppression d'un patient.

### `Views/Users/Index.cshtml`
**Rôle :** Liste des utilisateurs (Admin)  
**Description :** Tableau des utilisateurs avec filtres (recherche, rôle, statut) et actions CRUD.

### `Views/Users/Create.cshtml`
**Rôle :** Création utilisateur (Admin)  
**Description :** Formulaire création utilisateur avec sélection du rôle.

### `Views/Users/Edit.cshtml`
**Rôle :** Modification utilisateur (Admin)  
**Description :** Formulaire de modification des informations utilisateur.

### `Views/RendezVous/Index.cshtml`
**Rôle :** Liste des rendez-vous (Secrétaire)  
**Description :** Tableau des rendez-vous avec filtres (patient, médecin, statut, date) et actions.

### `Views/RendezVous/Create.cshtml`
**Rôle :** Création rendez-vous  
**Description :** Formulaire création rendez-vous avec sélection patient/médecin, date, chargement dynamique créneaux disponibles via API.

### `Views/RendezVous/Edit.cshtml`
**Rôle :** Modification rendez-vous  
**Description :** Formulaire de modification d'un rendez-vous.

### `Views/RendezVous/Details.cshtml`
**Rôle :** Détails rendez-vous  
**Description :** Affichage détaillé d'un rendez-vous.

### `Views/RendezVous/MesRendezVous.cshtml`
**Rôle :** Mes rendez-vous (Médecin/Patient)  
**Description :** Liste des rendez-vous filtrés selon l'utilisateur (médecin ou patient) avec filtres.

### `Views/DossierMedicals/Index.cshtml`
**Rôle :** Liste des dossiers médicaux (Secrétaire)  
**Description :** Tableau des dossiers médicaux avec filtre par nom patient.

### `Views/DossierMedicals/Create.cshtml`
**Rôle :** Création dossier médical  
**Description :** Formulaire création dossier médical pour un patient.

### `Views/DossierMedicals/Edit.cshtml`
**Rôle :** Modification dossier médical  
**Description :** Formulaire de modification des remarques d'un dossier médical.

### `Views/DossierMedicals/Consulter.cshtml`
**Rôle :** Consultation dossier médical  
**Description :** Affichage historique complet (consultations, ordonnances) d'un dossier médical.

### `Views/DossierMedicals/MonDossier.cshtml`
**Rôle :** Mon dossier médical (Patient)  
**Description :** Vue patient de son propre dossier médical complet.

### `Views/Consultations/Index.cshtml`
**Rôle :** Liste des consultations (Médecin)  
**Description :** Tableau des consultations avec filtres (patient, date).

### `Views/Consultations/Create.cshtml`
**Rôle :** Création consultation  
**Description :** Formulaire création consultation (dossier médical, diagnostic, notes).

### `Views/Consultations/Edit.cshtml`
**Rôle :** Modification consultation  
**Description :** Formulaire de modification d'une consultation.

### `Views/Consultations/Details.cshtml`
**Rôle :** Détails consultation  
**Description :** Affichage détaillé d'une consultation avec ordonnances et factures associées.

### `Views/Consultations/Delete.cshtml`
**Rôle :** Suppression consultation  
**Description :** Confirmation de suppression d'une consultation.

### `Views/Consultations/Imprimer.cshtml`
**Rôle :** Impression consultation  
**Description :** Vue formatée pour impression (sans layout) d'une consultation.

### `Views/Ordonnances/Index.cshtml`
**Rôle :** Liste des ordonnances (Médecin)  
**Description :** Tableau des ordonnances avec filtres (patient, date).

### `Views/Ordonnances/Create.cshtml`
**Rôle :** Création ordonnance  
**Description :** Formulaire création ordonnance avec ajout dynamique de détails (Type, Description, Dosage).

### `Views/Ordonnances/Edit.cshtml`
**Rôle :** Modification ordonnance  
**Description :** Formulaire de modification d'une ordonnance avec gestion dynamique des détails (validation côté client et serveur).

### `Views/Ordonnances/Details.cshtml`
**Rôle :** Détails ordonnance  
**Description :** Affichage détaillé d'une ordonnance avec tous ses détails.

### `Views/Ordonnances/Delete.cshtml`
**Rôle :** Suppression ordonnance  
**Description :** Confirmation de suppression d'une ordonnance.

### `Views/Ordonnances/Imprimer.cshtml`
**Rôle :** Impression ordonnance  
**Description :** Vue formatée pour impression (sans layout) d'une ordonnance avec tableau des détails.

### `Views/Ordonnances/MesOrdonnances.cshtml`
**Rôle :** Mes ordonnances (Patient)  
**Description :** Liste des ordonnances du patient connecté.

### `Views/Factures/Index.cshtml`
**Rôle :** Liste des factures (Secrétaire)  
**Description :** Tableau des factures avec actions (Create, Details, Imprimer).

### `Views/Factures/Create.cshtml`
**Rôle :** Création facture  
**Description :** Formulaire création facture (patient, consultation, montant).

### `Views/Factures/Details.cshtml`
**Rôle :** Détails facture  
**Description :** Affichage détaillé d'une facture avec historique des paiements.

### `Views/Factures/Payer.cshtml`
**Rôle :** Paiement facture (Patient)  
**Description :** Formulaire de paiement d'une facture (mode paiement, montant).

### `Views/Factures/MesFactures.cshtml`
**Rôle :** Mes factures (Patient)  
**Description :** Liste des factures du patient connecté avec statut et actions de paiement.

### `Views/Factures/Imprimer.cshtml`
**Rôle :** Impression facture  
**Description :** Vue formatée pour impression (sans layout) d'une facture.

### `Views/Alertes/Index.cshtml`
**Rôle :** Liste des alertes  
**Description :** Affichage des alertes non lues avec possibilité de marquer comme lue, filtres par type.

### `Views/Profile/Index.cshtml`
**Rôle :** Profil utilisateur  
**Description :** Affichage des informations du profil utilisateur.

### `Views/Profile/Edit.cshtml`
**Rôle :** Modification profil  
**Description :** Formulaire de modification du profil utilisateur (email, password).

### `Views/AdminDashboard/Index.cshtml`
**Rôle :** Vue dashboard admin  
**Description :** Vue partielle utilisée par le DashboardController pour le rôle Admin.

### `Views/SecretaireDashboard/Index.cshtml`
**Rôle :** Vue dashboard secrétaire  
**Description :** Vue partielle utilisée par le DashboardController pour le rôle Secretaire.

### `Views/MedecinDashboard/Index.cshtml`
**Rôle :** Vue dashboard médecin  
**Description :** Vue partielle utilisée par le DashboardController pour le rôle Medecin.

### `Views/PatientDashboard/Index.cshtml`
**Rôle :** Vue dashboard patient  
**Description :** Vue partielle utilisée par le DashboardController pour le rôle Patient.

---

## 📂 WWWROOT (Fichiers statiques)

### `wwwroot/css/site.css`
**Rôle :** Styles personnalisés  
**Description :** Feuille de styles CSS personnalisée pour l'application.

### `wwwroot/css/modern.css`
**Rôle :** Styles modernes  
**Description :** Styles CSS supplémentaires pour un design moderne.

### `wwwroot/js/site.js`
**Rôle :** Scripts JavaScript personnalisés  
**Description :** Scripts JavaScript pour fonctionnalités côté client (protection routes, actualisation alertes, etc.).

### `wwwroot/lib/`
**Rôle :** Bibliothèques tierces  
**Description :** Contient Bootstrap, jQuery, jQuery Validation, Bootstrap Icons, Animate.css, etc. (gérés via LibMan ou CDN).

### `wwwroot/favicon.ico`
**Rôle :** Icône du site  
**Description :** Icône affichée dans l'onglet du navigateur.

---

## 📂 MIGRATIONS (Base de données)

### `Migrations/20251213221652_InitialCreate.cs`
**Rôle :** Migration initiale  
**Description :** Crée toutes les tables initiales (Users, Patients, Medecins, Secretaires, RendezVous, DossierMedicals, Consultations, Ordonnances, OrdonnanceDetails, Factures, Paiements) avec leurs relations et contraintes.

### `Migrations/20251213221652_InitialCreate.Designer.cs`
**Rôle :** Métadonnées de migration  
**Description :** Fichier généré automatiquement par Entity Framework contenant les métadonnées de la migration.

### `Migrations/20251216201313_AddAlertesTable.cs`
**Rôle :** Migration table alertes  
**Description :** Ajoute la table Alertes avec ses relations vers Users et RendezVous.

### `Migrations/20251216201313_AddAlertesTable.Designer.cs`
**Rôle :** Métadonnées de migration  
**Description :** Métadonnées générées pour la migration AddAlertesTable.

### `Migrations/CabinetMedicalContextModelSnapshot.cs`
**Rôle :** Instantané du modèle  
**Description :** Représentation actuelle du modèle de données utilisé par EF Core pour générer les nouvelles migrations.

---

## 📂 DOCUMENTATION

### `CHANGELOG.md`
**Rôle :** Journal des changements  
**Description :** Historique des versions, nouvelles fonctionnalités, corrections de bugs.

### `DOCUMENTATION_PROJET.md`
**Rôle :** Documentation complète du projet  
**Description :** Documentation détaillée du projet, architecture, modèles, fonctionnalités, installation.

### `PROJET_FINAL_100.md`
**Rôle :** Documentation finale  
**Description :** Documentation finale du projet avec toutes les fonctionnalités implémentées.

### `RESUME_FONCTIONNALITES.md`
**Rôle :** Résumé des fonctionnalités  
**Description :** Liste résumée de toutes les fonctionnalités du projet.

### `EXPLICATION_SYSTEME_*.md` (9 fichiers)
**Rôle :** Explications détaillées des systèmes  
**Description :** Documents expliquant en détail chaque système (Alertes, Authentification, Rendez-vous, Consultation, Ordonnance, Facturation, Dossier Médical, Gestion Patients, Dashboard).

### `resume/RESUME_SYSTEME_*.md` (9 fichiers)
**Rôle :** Résumés courts des systèmes  
**Description :** Résumés très courts de chaque système (rôle, technologies, localisation, flux principal).

### `resume/INDEX_RESUMES.md`
**Rôle :** Index des résumés  
**Description :** Index et guide des fichiers de résumés.

### `create_alertes_table.sql`
**Rôle :** Script SQL manuel  
**Description :** Script SQL alternatif pour créer la table Alertes (non utilisé si migrations sont appliquées).

---

## 📂 RAPPORT

### `Rapport_Cabinet_Medical.md`
**Rôle :** Rapport de projet  
**Description :** Rapport complet du projet destiné à une présentation universitaire, incluant architecture, modèles, fonctionnalités, code examples, etc.

---

**Note :** Les dossiers `bin/` et `obj/` contiennent les fichiers compilés et temporaires générés par .NET et ne sont pas versionnés.

