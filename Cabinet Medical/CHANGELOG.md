# 📋 CHANGELOG - Cabinet Medical Management System

## Version 1.3 - Décembre 2024

### ✨ Nouvelles Fonctionnalités

#### 🔍 Système de Filtres et Recherche Avancée
- **Fichiers modifiés**:
  - `Controllers/UsersController.cs` - Ajout de filtres (recherche, rôle, statut)
  - `Controllers/RendezVousController.cs` - Filtres pour secrétaire et mes rendez-vous
  - `Controllers/PatientsController.cs` - Filtre de recherche par nom/email/téléphone
  - `Controllers/DossierMedicalsController.cs` - Filtre par nom de patient
  - `Controllers/ConsultationsController.cs` - Filtres par patient et date
  - `Controllers/OrdonnancesController.cs` - Filtres par patient et date
  - `Views/Users/Index.cshtml` - Interface de filtres pour Admin
  - `Views/RendezVous/Index.cshtml` - Filtres multi-critères pour secrétaire
  - `Views/RendezVous/MesRendezVous.cshtml` - Filtres pour médecin et patient
  - `Views/Patients/Index.cshtml` - Recherche rapide
  - `Views/DossierMedicals/Index.cshtml` - Recherche par patient
  - `Views/Consultations/Index.cshtml` - Filtres pour médecin
  - `Views/Ordonnances/Index.cshtml` - Filtres pour médecin

- **Fonctionnalités**:
  - **Admin - Utilisateurs** :
    - Recherche par username ou email
    - Filtre par rôle (Admin, Médecin, Secrétaire, Patient)
    - Filtre par statut (Actif, Inactif)
    - Combinaison de filtres simultanés
  - **Secrétaire - Rendez-vous** :
    - Recherche par nom de patient
    - Recherche par nom de médecin
    - Filtre par statut (Planifié, Terminé, Annulé)
    - Filtre par date
  - **Secrétaire - Patients** :
    - Recherche multi-critères (nom, prénom, email, téléphone)
  - **Secrétaire - Dossiers médicaux** :
    - Recherche par nom de patient
  - **Médecin - Consultations** :
    - Recherche par nom de patient
    - Filtre par date
  - **Médecin - Ordonnances** :
    - Recherche par nom de patient
    - Filtre par date
  - **Médecin - Mes Rendez-vous** :
    - Recherche par nom de patient
    - Filtre par statut et date
  - **Patient - Mes Rendez-vous** :
    - Recherche par nom de médecin
    - Filtre par statut et date
  - **Bouton de réinitialisation** : Visible quand des filtres sont actifs
  - **Préservation des filtres** : Les valeurs restent dans les champs après recherche

#### 👤 Création Simplifiée de Patient par la Secrétaire
- **Fichiers modifiés**:
  - `Controllers/PatientsController.cs` - Création simultanée User + Patient
  - `Views/Patients/Create.cshtml` - Formulaire unifié avec tous les champs
  - `Views/RendezVous/Create.cshtml` - Bouton "Nouveau Patient" avec retour automatique

- **Fonctionnalités**:
  - **Création en une étape** : Plus besoin de sélectionner un utilisateur existant
  - **Formulaire complet** :
    - Informations de connexion (Username, Email, Password) - obligatoires
    - Informations personnelles (Nom, Prénom, Date de naissance, Téléphone, Adresse, Antécédents médicaux)
  - **Retour automatique** : Après création, retour au formulaire de rendez-vous avec patient pré-sélectionné
  - **Validation complète** : Vérification unicité username/email, validation champs obligatoires
  - **Message d'information** : Notification de succès lors du retour au formulaire RDV

#### 🔧 Corrections Techniques Majeures

##### Suppression de Consultation avec Cascade Complète
- **Fichiers modifiés**:
  - `Controllers/ConsultationsController.cs` - Méthode `DeleteConfirmed` améliorée

- **Problème résolu**:
  - **Erreur FK** : Violation de contrainte lors de suppression d'une consultation avec factures liées
  - **Solution** : Suppression en cascade dans l'ordre :
    1. Paiements des factures liées
    2. Factures liées à la consultation
    3. Détails des ordonnances
    4. Ordonnances liées
    5. Consultation
  - **Validation** : Gestion des erreurs avec messages clairs

##### Modification d'Ordonnance
- **Fichiers modifiés**:
  - `Controllers/OrdonnancesController.cs` - Validation améliorée dans `Edit`
  - `Views/Ordonnances/Edit.cshtml` - Correction syntaxe Razor et validation JavaScript

- **Problèmes résolus**:
  - **Syntaxe Razor** : Correction des attributs `selected` dans les `<option>`
  - **Validation** : Ajout validation côté serveur et client
  - **Gestion des erreurs** : Messages d'erreur clairs et affichage des erreurs
  - **Gestion des valeurs nulles** : Protection contre les valeurs null/empty

##### Système d'Alertes Amélioré
- **Fichiers modifiés**:
  - `Controllers/RendezVousController.cs` - Méthode `CreateAlerteAnnulation` améliorée

- **Améliorations**:
  - **Alertes pour médecin** : Lorsqu'une secrétaire annule un RDV, le médecin reçoit aussi une alerte
  - **Alertes complètes** :
    - Patient annule → Alertes pour Secrétaire ET Médecin
    - Secrétaire annule → Alertes pour Patient ET Médecin
  - **Messages personnalisés** : Messages différents selon le destinataire

### 🔧 Améliorations Techniques

#### Corrections de Bugs
- **Syntaxe Razor** : Correction de tous les attributs `selected` dans les filtres
- **Compilation** : Résolution des erreurs `CS0111` (méthodes dupliquées)
- **Validation** : Amélioration de la gestion des erreurs dans tous les formulaires
- **Null Safety** : Protection contre les valeurs null dans les vues

#### Optimisations
- **Requêtes optimisées** : Utilisation de `IQueryable` pour les filtres (filtrage en base de données)
- **Expérience utilisateur** : Préservation des filtres actifs avec indication visuelle
- **Performance** : Filtres appliqués côté serveur pour réduire la charge client

---

## Version 1.2 - Décembre 2024

### ✨ Nouvelles Fonctionnalités

#### 🔔 Système d'Alertes et Notifications
- **Fichiers créés/modifiés**:
  - `Models/Alerte.cs` - Nouveau modèle pour les alertes
  - `Controllers/AlertesController.cs` - Gestion des alertes
  - `Views/Alertes/Index.cshtml` - Page de consultation des alertes
  - `Views/Shared/_Layout.cshtml` - Badge de notification en temps réel
  - `Data/CabinetMedicalContext.cs` - Configuration de la table Alertes
  - `Migrations/20251216201313_AddAlertesTable.cs` - Migration pour la table Alertes

- **Fonctionnalités**:
  - **Alertes d'annulation** : Si un patient annule son RDV, la secrétaire reçoit une alerte et vice versa
  - **Rappels automatiques** : Création automatique d'alertes de rappel 24h avant chaque RDV pour les patients
  - **Badge de notification** : Affichage du nombre d'alertes non lues dans le header
  - **Actualisation automatique** : Vérification périodique des alertes non lues (toutes les 30 secondes)
  - **Marquer comme lu** : Possibilité de marquer une alerte comme lue en cliquant dessus
  - **Filtrage** : Alertes triées par date de création (plus récentes en premier)

#### 📋 Gestion Améliorée des Dossiers Médicaux
- **Fichiers modifiés**:
  - `Controllers/PatientsController.cs` - `Index()` inclut maintenant les dossiers médicaux
  - `Views/Patients/Index.cshtml` - Affichage du statut du dossier médical avec badge
  - `Controllers/DossierMedicalsController.cs` - `Create()` pré-rempli avec patientId

- **Fonctionnalités**:
  - **Indicateur visuel** : Badge vert "Oui" / rouge "Non" pour indiquer la présence d'un dossier médical
  - **Bouton rapide** : Bouton "Créer dossier médical" directement dans la liste des patients (uniquement pour patients sans dossier)
  - **Lien direct** : Création de dossier médical avec patientId pré-rempli depuis la liste

#### 🗓️ Annulation de Rendez-vous par les Patients
- **Fichiers modifiés**:
  - `Controllers/RendezVousController.cs` - Méthode `Annuler` accessible par les patients
  - `Views/RendezVous/MesRendezVous.cshtml` - Bouton "Annuler" pour les RDV "Planifié"
  - `Views/RendezVous/Index.cshtml` - Confirmation avant annulation pour secrétaire

- **Fonctionnalités**:
  - **Auto-annulation** : Les patients peuvent annuler leurs propres RDV avec statut "Planifié"
  - **Confirmation** : Dialogue de confirmation avant annulation
  - **Alerte automatique** : Lorsqu'un patient annule, une alerte est créée pour la secrétaire
  - **Validation** : Seuls les RDV "Planifié" peuvent être annulés

#### 👨‍⚕️ Affichage de la Disponibilité du Médecin
- **Fichiers modifiés**:
  - `Controllers/RendezVousController.cs` - Méthode `GetAvailableSlots` améliorée
  - `Views/RendezVous/Create.cshtml` - Affichage de la disponibilité en temps réel

- **Fonctionnalités**:
  - **Statistiques en temps réel** : Affichage du nombre de créneaux libres et du taux d'occupation
  - **Calcul automatique** : Basé sur les créneaux de 30 minutes (08:00-17:00, Lundi-Vendredi)
  - **Mise à jour dynamique** : Actualisation lors du changement de médecin ou de date
  - **Indicateurs visuels** : Affichage clair de la disponibilité (créneaux libres / créneaux totaux)

#### ⏰ Système de Rappels Automatiques
- **Fichiers modifiés**:
  - `Controllers/RendezVousController.cs` - Nouvelle méthode `CheckRappels` (API)
  - `Views/Shared/_Layout.cshtml` - Script JavaScript pour vérifier les rappels

- **Fonctionnalités**:
  - **Vérification automatique** : Contrôle périodique des RDV dans les 24 prochaines heures
  - **Création d'alertes** : Génération automatique d'alertes de type "RappelRDV" pour les patients
  - **Déduplication** : Prévention des alertes de rappel en double
  - **Notification navigateur** : Possibilité d'envoyer des notifications du navigateur (si autorisé)

### 🔧 Améliorations Techniques

#### Base de Données
- **Migration créée** : `AddAlertesTable` pour créer la table `Alertes`
- **Relations configurées** : Relations `Alerte -> User` et `Alerte -> RendezVous` avec `DeleteBehavior.NoAction`
- **Index créés** : Index sur `UserId` et `RendezVousId` pour optimiser les requêtes

#### Corrections
- **Relation DossierMedical-Patient** : Correction de la relation dans `CabinetMedicalContext.cs` (WithOne au lieu de WithMany)

---

## Version 1.1 - Décembre 2024

### ✨ Nouvelles Fonctionnalités

#### 🔐 Système d'Inscription pour Patients
- **Fichiers modifiés/créés**:
  - `Controllers/AccountController.cs` - Ajout des méthodes `Register` (GET/POST)
  - `Views/Account/Register.cshtml` - Nouveau formulaire d'inscription
  - `Views/Account/Login.cshtml` - Ajout du bouton "S'inscrire"

- **Fonctionnalités**:
  - Formulaire d'inscription complet avec tous les champs obligatoires
  - Validation côté client et serveur
  - Création automatique dans les tables `Users` et `Patients`
  - Design moderne avec animations
  - Redirection vers login après inscription réussie

#### 📅 Système de Prise de Rendez-vous par les Patients
- **Fichiers modifiés**:
  - `Controllers/RendezVousController.cs`:
    - Modification de `Create` (GET) pour permettre l'accès aux patients
    - Modification de `Create` (POST) avec validation complète
    - Nouvelle méthode `ValidateAppointment` pour les règles métier
    - Nouvelle méthode `GetAvailableSlots` (API) pour les créneaux disponibles
  - `Views/RendezVous/Create.cshtml`:
    - Interface améliorée avec sélection dynamique des créneaux
    - Chargement automatique des créneaux disponibles
    - Validation JavaScript pour les jours ouvrables
  - `Views/Shared/_Layout.cshtml`:
    - Ajout du lien "Prendre un rendez-vous" pour les patients

- **Règles métier implémentées**:
  - ✅ Jours ouvrables uniquement : Lundi-Vendredi
  - ✅ Heures : 08:00 - 17:00 uniquement
  - ✅ Créneaux de 30 minutes (08:00, 08:30, 09:00... jusqu'à 16:30)
  - ✅ Prévention des doublons (vérification si créneau déjà pris avec le même médecin)
  - ✅ Filtrage automatique des heures passées si date = aujourd'hui
  - ✅ Buffer de 30 minutes (impossible de réserver dans les 30 prochaines minutes)

### 🔧 Améliorations Techniques

#### Configuration et Infrastructure
- **Fichiers modifiés**:
  - `Program.cs` - Ajout de `app.UseStaticFiles()` pour servir les fichiers CSS/JS

- **Problème résolu**:
  - Erreurs 404 sur les fichiers CSS corrigées

#### Base de Données et EF Core
- **Fichiers modifiés**:
  - `Data/CabinetMedicalContext.cs`:
    - Correction relation `Paiement -> Facture` : `WithMany(f => f.Paiements)`
    - Correction relation `OrdonnanceDetail -> Ordonnance` : `WithMany(o => o.OrdonnanceDetails)`

- **Problèmes résolus**:
  - Erreur SQL `FactureId1` corrigée
  - Relations EF Core correctement configurées

#### Design et Interface Utilisateur
- **Fichiers modifiés**:
  - `wwwroot/css/modern.css`:
    - Amélioration des stats-cards avec gradients et animations
    - Nouveaux styles pour feature-cards
    - Ajout de `!important` pour forcer les styles
    - Versioning CSS (`?v=2.0`)

  - `Views/Dashboard/Admin.cshtml` - Statistiques en temps réel
  - `Views/Dashboard/Secretaire.cshtml` - Statistiques en temps réel
  - `Views/Dashboard/Medecin.cshtml` - Statistiques personnalisées
  - `Views/Dashboard/Patient.cshtml` - Statistiques personnelles
  - `Views/AdminDashboard/Index.cshtml` - Statistiques Admin

- **Améliorations**:
  - Stats-cards avec gradients colorés par type
  - Icônes plus grandes avec gradients
  - Effets hover avec animations
  - Feature-cards avec design moderne
  - Transitions fluides

#### Contrôleurs et Vues
- **Fichiers modifiés**:
  - `Controllers/DashboardController.cs`:
    - Méthodes async pour charger les statistiques
    - Calcul des statistiques dans les contrôleurs (pas dans les vues)
    - Passage des données via ViewBag

  - `Controllers/AdminDashboardController.cs`:
    - Chargement des statistiques Admin

- **Problèmes résolus**:
  - Erreurs `CountAsync` dans les vues Razor corrigées
  - Logique déplacée vers les contrôleurs

#### Corrections de Bugs
- **Fichiers modifiés**:
  - `Views/Account/Login.cshtml` - Correction `@keyframes` → `@@keyframes`
  - `Views/Account/Register.cshtml` - Correction `@keyframes` → `@@keyframes`

- **Problèmes résolus**:
  - Erreurs de compilation CS0103 corrigées

---

## Version 1.0 - Décembre 2024

### ✅ Version Initiale

- ✅ Authentification et gestion des rôles
- ✅ Gestion complète des utilisateurs (Admin)
- ✅ Gestion des patients (Secrétaire)
- ✅ Gestion des rendez-vous (Secrétaire)
- ✅ Gestion des dossiers médicaux
- ✅ Gestion des consultations (Médecin)
- ✅ Gestion des ordonnances (Médecin)
- ✅ Gestion des factures et paiements
- ✅ Design moderne 2025
- ✅ Tous les use cases implémentés

---

## 📝 Notes de Migration

### Pour migrer vers la version 1.1 :

1. **Base de données** :
   ```sql
   -- Vérifier que les relations sont correctes
   -- Si colonne FactureId1 existe, la supprimer :
   ALTER TABLE Paiements DROP COLUMN IF EXISTS FactureId1;
   ```

2. **Fichiers statiques** :
   - S'assurer que `app.UseStaticFiles()` est dans `Program.cs`
   - Vider le cache du navigateur (Ctrl+Shift+R)

3. **Nouvelles fonctionnalités** :
   - Les patients peuvent maintenant s'inscrire via `/Account/Register`
   - Les patients peuvent prendre des rendez-vous via `/RendezVous/Create`

---

---

## 📝 Notes de Migration

### Pour migrer vers la version 1.2 :

1. **Base de données** :
   ```bash
   # Exécuter la migration pour la table Alertes
   dotnet ef database update
   # Ou exécuter manuellement le script SQL : create_alertes_table.sql
   ```

2. **Nouvelles fonctionnalités** :
   - Le système d'alertes est automatiquement actif après la migration
   - Les rappels sont vérifiés automatiquement toutes les minutes (configurable)
   - Les alertes d'annulation sont créées automatiquement lors des annulations

3. **Configuration** :
   - La vérification des rappels peut être désactivée/modifiée dans `_Layout.cshtml`
   - Le délai de vérification est configuré dans le JavaScript (par défaut : 1 minute)

### Pour migrer vers la version 1.3 :

1. **Aucune migration base de données requise** : La version 1.3 n'ajoute pas de nouvelles tables

2. **Nouvelles fonctionnalités disponibles immédiatement** :
   - Les filtres de recherche sont disponibles sur toutes les pages concernées
   - La création simplifiée de patient est active dans le formulaire de rendez-vous
   - Les alertes pour le médecin lors d'annulation sont automatiques

3. **Améliorations** :
   - Meilleure expérience utilisateur avec les filtres
   - Workflow simplifié pour la création de patients
   - Corrections de bugs importantes (suppression consultation, modification ordonnance)

---

**Dernière mise à jour** : Décembre 2024 (Version 1.3)

