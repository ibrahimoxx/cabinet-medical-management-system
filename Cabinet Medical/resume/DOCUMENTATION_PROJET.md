# 🏥 PROJET : GESTION D'UN CABINET MÉDICAL

## 📌 INFORMATIONS GÉNÉRALES

**Nom du projet :** Cabinet Medical Management System  
**Technologie :** ASP.NET Core MVC 10.0  
**Base de données :** SQL Server (Entity Framework Core 10.0.1)  
**Interface :** Bootstrap 5.3.3 + Animate.css  
**Pattern :** MVC (Model-View-Controller)

---

## 🎯 OBJECTIF GÉNÉRAL

Développer une application web moderne et sécurisée pour la **gestion complète d'un cabinet médical**, permettant :

- ✅ Une **gestion centralisée** des utilisateurs et des patients
- ✅ La **planification et le suivi** des rendez-vous médicaux
- ✅ Le **suivi médical** avec dossiers, consultations et ordonnances
- ✅ La **facturation et gestion des paiements**
- ✅ Un **accès sécurisé** selon les rôles et responsabilités
- ✅ Une **interface utilisateur moderne** et intuitive (2025)

---

## 🎯 OBJECTIFS SPÉCIFIQUES

### 1. Gestion des Utilisateurs
- ✅ Authentification sécurisée par session
- ✅ Système de rôles (Admin, Médecin, Secrétaire, Patient)
- ✅ Gestion complète des utilisateurs par l'administrateur
- ✅ Profils utilisateurs personnalisables

### 2. Gestion des Patients
- ✅ Création et modification des dossiers patients
- ✅ Historique médical complet
- ✅ Suivi des informations personnelles et médicales

### 3. Gestion des Rendez-vous
- ✅ Planification des rendez-vous médicaux
- ✅ **Prise de rendez-vous en ligne par les patients** (NOUVEAU)
- ✅ **Annulation de RDV par les patients** (NOUVEAU - Version 1.2)
- ✅ **Validation des horaires** : Lundi-Vendredi, 08:00-17:00, créneaux de 30 min
- ✅ **Prévention des doublons** : Impossible de réserver un créneau déjà pris
- ✅ **Affichage de la disponibilité** : Créneaux libres et taux d'occupation du médecin (NOUVEAU)
- ✅ Suivi du statut (Planifié, Annulé, Terminé)
- ✅ Association Patient/Médecin
- ✅ **API des créneaux disponibles en temps réel**

### 4. Gestion Médicale
- ✅ Création de dossiers médicaux
- ✅ **Création rapide depuis la liste des patients** (NOUVEAU - Version 1.2)
- ✅ **Indicateur visuel du statut du dossier** (Badge Oui/Non) (NOUVEAU)
- ✅ Enregistrement des consultations
- ✅ Génération d'ordonnances (Médicaments, Analyses, Radiologie)
- ✅ Historique complet des consultations

### 7. Système d'Alertes et Notifications (NOUVEAU - Version 1.2)
- ✅ **Alertes d'annulation** : Notifications automatiques lors de l'annulation d'un RDV
  - Patient annule → Secrétaire reçoit une alerte
  - Secrétaire annule → Patient reçoit une alerte
- ✅ **Rappels automatiques** : Alertes créées 24h avant chaque RDV pour les patients
- ✅ **Badge de notification** : Affichage du nombre d'alertes non lues en temps réel
- ✅ **Consultation des alertes** : Page dédiée pour consulter toutes les alertes
- ✅ **Marquer comme lu** : Fonctionnalité pour marquer les alertes comme lues

### 5. Facturation et Paiements
- ✅ Génération automatique de factures
- ✅ Suivi des paiements (En ligne, Espèces)
- ✅ Historique financier par patient

### 6. Sécurité et Contrôle d'Accès
- ✅ Authentification par session
- ✅ Autorisation basée sur les rôles
- ✅ Protection des données médicales sensibles

---

## 👥 ACTEURS DU SYSTÈME

| Acteur | Description | Responsabilités Principales |
|--------|-------------|----------------------------|
| **Admin** | Administrateur système | Gestion globale des utilisateurs, contrôle du système |
| **Secrétaire** | Personnel administratif | Gestion des patients, planification des rendez-vous |
| **Médecin** | Personnel médical | Consultations, dossiers médicaux, ordonnances |
| **Patient** | Utilisateur final | Consultation de son dossier médical et historique |

---

## 🧠 ARCHITECTURE GÉNÉRALE

```
┌─────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                    │
│  ASP.NET Core MVC Views (Razor) + Bootstrap 5 + JS      │
├─────────────────────────────────────────────────────────┤
│                   APPLICATION LAYER                      │
│        Controllers + Services + Filters                  │
├─────────────────────────────────────────────────────────┤
│                      DATA LAYER                          │
│    Entity Framework Core + CabinetMedicalContext        │
├─────────────────────────────────────────────────────────┤
│                   DATABASE LAYER                         │
│                    SQL Server Database                   │
└─────────────────────────────────────────────────────────┘
```

### Technologies Utilisées

- **Frontend :**
  - ASP.NET Core MVC (Razor Views)
  - Bootstrap 5.3.3
  - Bootstrap Icons
  - Animate.css 4.1.1
  - jQuery + jQuery Validation

- **Backend :**
  - ASP.NET Core 10.0
  - Entity Framework Core 10.0.1
  - SQL Server

- **Sécurité :**
  - Sessions ASP.NET Core
  - Contrôle d'accès par rôles (RoleController)
  - Protection CSRF (AntiForgeryToken)

---

## 🧩 DIAGRAMME DE CAS D'UTILISATION (USE CASE DIAGRAM)

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CAS D'UTILISATION                             │
└─────────────────────────────────────────────────────────────────────┘

┌──────────┐
│  Admin   │
└────┬─────┘
     │
     ├───► Consulter utilisateurs
     │         ├─► Ajouter utilisateur <<extend>>
     │         ├─► Modifier utilisateur <<extend>>
     │         └─► Activer/Désactiver utilisateur <<extend>>
     │
     ├───► Supprimer utilisateur (sauf Admin)
     │
     ├───► Accéder au dashboard admin
     │
     └───► Modifier son profil

┌──────────────┐
│  Secrétaire  │
└──────┬───────┘
       │
       ├───► Consulter patients
       │         ├─► Ajouter patient <<extend>>
       │         ├─► Modifier patient <<extend>>
       │         └─► Supprimer patient <<extend>>
       │
       ├───► Planifier rendez-vous <<include>> (Consulter patient)
       │
       ├───► Modifier rendez-vous
       │
       ├───► Annuler rendez-vous
       │
       ├───► Consulter dossier médical (lecture seule)
       │
       └───► Modifier son profil

┌──────────┐
│  Médecin │
└────┬─────┘
     │
     ├───► Consulter rendez-vous
     │
     ├───► Consulter dossier médical
     │         ├─► Ajouter consultation <<include>>
     │         └─► Ajouter ordonnance <<extend>>
     │
     ├───► Mettre à jour dossier médical
     │
     ├───► Consulter historique patient
     │
     └───► Modifier son profil

┌──────────┐
│  Patient │
└────┬─────┘
     │
     ├───► Consulter son dossier médical (lecture seule)
     │
     ├───► Consulter ses rendez-vous (lecture seule)
     │
     ├───► Consulter ses ordonnances (lecture seule)
     │
     ├───► Consulter ses factures (lecture seule)
     │
     └───► Modifier son profil
```

### Légende
- **<<extend>>** : Extension optionnelle
- **<<include>>** : Inclusion obligatoire

---

## 🗂️ DIAGRAMME DE CLASSES (CLASS DIAGRAM)

```
┌─────────────────────────────────────────────────────────────────────┐
│                        MODÈLE DE DONNÉES                             │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐
│         User            │
├─────────────────────────┤
│ + Id : int              │
│ + Username : string     │
│ + PasswordHash : string │
│ + Email : string        │
│ + Role : string         │
│ + IsActive : bool       │
│ + CreatedAt : DateTime  │
└────────┬────────────────┘
         │
         │ 1
         │
    ┌────┴─────────────────────────────────────┐
    │                                          │
    │ 1                                        │ 1
    │                                          │
┌───┴──────────┐                     ┌────────┴────────┐
│   Patient    │                     │     Medecin     │
├──────────────┤                     ├─────────────────┤
│ + Id : int   │                     │ + Id : int      │
│ + UserId     │                     │ + UserId        │
│ + Nom        │                     │ + Nom           │
│ + Prenom     │                     │ + Prenom        │
│ + Adresse    │                     │ + Specialite    │
│ + Telephone  │                     │ + Telephone     │
│ + ...        │                     └────────┬────────┘
└──────┬───────┘                              │
       │                                      │
       │ *                                    │ *
       │                                      │
┌──────┴──────────┐                  ┌───────┴─────────┐
│  RendezVous     │                  │  Consultation   │
├─────────────────┤                  ├─────────────────┤
│ + Id            │                  │ + Id            │
│ + PatientId     │                  │ + DossierMedId  │
│ + MedecinId     │                  │ + MedecinId     │
│ + DateRdv       │                  │ + DateConsult   │
│ + HeureRdv      │                  │ + Diagnostic    │
│ + Statut        │                  │ + Notes         │
└─────────────────┘                  └────────┬────────┘
                                             │
                                             │ 1
                                             │
                                    ┌────────┴────────┐
                                    │   Ordonnance    │
                                    ├─────────────────┤
                                    │ + Id            │
                                    │ + ConsultationId│
                                    │ + DateOrd       │
                                    └────────┬────────┘
                                             │
                                             │ 1
                                             │ *
                                    ┌────────┴──────────┐
                                    │ OrdonnanceDetail  │
                                    ├───────────────────┤
                                    │ + Id              │
                                    │ + OrdonnanceId    │
                                    │ + Type            │
                                    │ + Description     │
                                    │ + Dosage          │
                                    └───────────────────┘

┌───────────────┐
│  DossierMed   │
├───────────────┤
│ + Id          │
│ + PatientId   │
│ + DateCreation│
│ + Remarques   │
└───────┬───────┘
        │
        │ 1
        │
        │ *
┌───────┴───────────┐
│   Consultation    │
└───────────────────┘

┌───────────────────┐
│      Facture      │
├───────────────────┤
│ + Id              │
│ + PatientId       │
│ + ConsultationId  │
│ + Montant : decimal│
│ + DateFacture     │
│ + Statut          │
└────────┬──────────┘
         │
         │ 1
         │
         │ *
┌────────┴─────────┐
│     Paiement     │
├──────────────────┤
│ + Id             │
│ + FactureId      │
│ + ModePaiement   │
│ + Montant        │
│ + DatePaiement   │
└──────────────────┘

┌──────────────┐
│  Secretaire  │
├──────────────┤
│ + Id         │
│ + UserId     │
│ + Nom        │
│ + Prenom     │
│ + Telephone  │
└──────────────┘
```

### Relations Principales

1. **User ↔ Patient/Medecin/Secretaire** : Relation 1-1
2. **Patient ↔ RendezVous** : Relation 1-*
3. **Patient ↔ DossierMedical** : Relation 1-1
4. **DossierMedical ↔ Consultation** : Relation 1-*
5. **Consultation ↔ Ordonnance** : Relation 1-*
6. **Ordonnance ↔ OrdonnanceDetail** : Relation 1-*
7. **Consultation ↔ Facture** : Relation 1-1
8. **Facture ↔ Paiement** : Relation 1-*

---

## 🔄 DERNIÈRES MODIFICATIONS

### Version 1.3 - Décembre 2024

#### ✨ Nouvelles Fonctionnalités

**Système de Filtres et Recherche Avancée**
- Filtres multi-critères pour Admin, Secrétaire, Médecin, Patient
- Recherche par nom, email, téléphone, date, statut
- Combinaison de filtres simultanés
- Préservation des filtres actifs

**Création Simplifiée de Patient**
- Création User + Patient en une seule étape
- Formulaire unifié avec tous les champs
- Intégration avec formulaire de rendez-vous
- Retour automatique avec pré-sélection

**Corrections Techniques**
- Suppression consultation : Cascade complète (paiements → factures)
- Modification ordonnance : Validation améliorée
- Alertes médecin : Notifications complètes pour toutes les parties

---

## 🔄 DERNIÈRES MODIFICATIONS (Version 1.2)

### ✨ Nouvelles Fonctionnalités (Décembre 2024)

#### 1. Système d'Alertes et Notifications
- **Modèle** : `Alerte.cs` avec Type, Message, UserId, RendezVousId, EstLue, DateCreation
- **Controller** : `AlertesController.cs` pour la gestion des alertes
- **Vue** : `Views/Alertes/Index.cshtml` pour consulter les alertes
- **Fonctionnalités** :
  - Alertes d'annulation de RDV (bidirectionnelles)
  - Rappels automatiques 24h avant les RDV
  - Badge de notification en temps réel
  - Vérification périodique des rappels

#### 2. Améliorations UX
- **Liste des patients** : Affichage du statut du dossier médical (Badge Oui/Non)
- **Bouton rapide** : Création de dossier médical directement depuis la liste
- **Annulation patient** : Possibilité pour les patients d'annuler leurs RDV
- **Disponibilité médecin** : Affichage des créneaux libres et du taux d'occupation

---

## 🔄 VERSION 1.1 (Décembre 2024)

### ✨ Nouvelles Fonctionnalités

#### 1. Système d'Inscription pour Patients
- Formulaire d'inscription complet (`/Account/Register`)
- Tous les champs obligatoires validés
- Création automatique dans `Users` et `Patients`
- Design moderne avec validation en temps réel

#### 2. Système de Prise de Rendez-vous par les Patients
- **Prise de rendez-vous en ligne** pour les patients
- **Validation des horaires** :
  - Lundi-Vendredi uniquement
  - Heures 08:00 - 17:00
  - Créneaux de 30 minutes
- **Prévention des doublons** :
  - Vérification automatique si créneau déjà pris
  - Empêche 2 patients de réserver le même créneau avec le même médecin
- **API des créneaux disponibles** :
  - Endpoint `/RendezVous/GetAvailableSlots`
  - Chargement dynamique selon médecin et date
  - Filtrage automatique des heures passées

#### 3. Améliorations Techniques
- Configuration fichiers statiques (`app.UseStaticFiles()`)
- Correction relations EF Core (`Facture.Paiements`, `Ordonnance.OrdonnanceDetails`)
- Design CSS moderne amélioré (stats-cards, feature-cards)
- Dashboards avec statistiques en temps réel

---

## 🔐 SÉCURITÉ & RÈGLES MÉTIER

### Authentification
- ✅ Système d'authentification par session
- ✅ Stockage du rôle et du nom d'utilisateur en session
- ✅ Redirection automatique si non authentifié

### Autorisation
- ✅ Contrôle d'accès basé sur les rôles (RoleController)
- ✅ Filtres personnalisés par rôle
- ✅ Protection des actions selon le rôle

### Règles Métier

#### Admin
- ❌ Ne peut jamais être supprimé
- ❌ Ne peut jamais être désactivé
- ❌ Son rôle ne peut pas être modifié
- ✅ Accès complet à la gestion des utilisateurs

#### Données Médicales
- ✅ Suppression en cascade contrôlée
- ✅ Conservation des données historiques
- ✅ Contraintes de clés étrangères avec `DeleteBehavior.NoAction`

#### Validation
- ✅ Validation côté client (jQuery Validation)
- ✅ Validation côté serveur (Data Annotations)
- ✅ Vérification de l'unicité (username, email)

---

## 🎨 INTERFACE UTILISATEUR (UI/UX)

### Design Moderne (2025)
- ✅ Interface Bootstrap 5.3.3
- ✅ Animations fluides (Animate.css)
- ✅ Sidebar responsive avec gradient
- ✅ Cards avec ombres modernes
- ✅ Icons Bootstrap Icons

### Navigation
- ✅ Sidebar fixe avec navigation par rôle
- ✅ Topbar avec informations utilisateur
- ✅ Breadcrumbs et navigation intuitive

### Responsive
- ✅ Design adaptatif mobile/tablette/desktop
- ✅ Layout flexible selon la taille d'écran

---

## 📊 ÉTAT D'AVANCEMENT DU PROJET

### ✅ FONCTIONNALITÉS TERMINÉES

#### Authentification & Sécurité
- [x] Système de login/logout
- [x] Gestion des sessions
- [x] Contrôle d'accès par rôles
- [x] Protection CSRF

#### Gestion des Utilisateurs (Admin)
- [x] Liste des utilisateurs
- [x] Ajout d'utilisateur avec champs dynamiques
- [x] Modification d'utilisateur
- [x] Activation/Désactivation
- [x] Suppression avec cascade contrôlée
- [x] Validation des données

#### Profils Utilisateurs
- [x] Consultation du profil
- [x] Modification du profil (tous rôles)
- [x] Mise à jour des informations personnelles

#### Architecture & Base de Données
- [x] Modèles de données complets
- [x] Relations Entity Framework configurées
- [x] Migrations initiales
- [x] Context configuré avec précision décimale

#### Interface Utilisateur
- [x] Layout principal avec sidebar
- [x] Dashboards par rôle
- [x] Formulaires avec validation
- [x] Messages de succès/erreur

### 🔜 FONCTIONNALITÉS À DÉVELOPPER

#### Secrétaire
- [ ] Gestion complète des patients (CRUD)
- [ ] Planification des rendez-vous
- [ ] Modification/Annulation des rendez-vous
- [ ] Consultation des dossiers médicaux (lecture)

#### Médecin
- [ ] Consultation des rendez-vous du jour
- [ ] Création et gestion des consultations
- [ ] Génération d'ordonnances avec détails
- [ ] Mise à jour des dossiers médicaux
- [ ] Historique des consultations par patient

#### Patient
- [ ] Consultation de son dossier médical
- [ ] Consultation de ses rendez-vous
- [ ] Consultation de ses ordonnances
- [ ] Consultation de ses factures et paiements

#### Sécurité Avancée
- [ ] Hashage des mots de passe (BCrypt/PBKDF2)
- [ ] Migration vers ASP.NET Core Identity (optionnel)
- [ ] Logging des actions sensibles

#### Fonctionnalités Avancées
- [ ] Statistiques et rapports
- [ ] Recherche avancée
- [ ] Export de données (PDF, Excel)
- [ ] Notifications système
- [ ] Calendrier interactif pour rendez-vous

---

## 📁 STRUCTURE DU PROJET

```
Cabinet Medical/
├── Controllers/
│   ├── AccountController.cs          # Authentification
│   ├── AdminDashboardController.cs   # Dashboard Admin
│   ├── DashboardController.cs        # Routage par rôle
│   ├── HomeController.cs             # Page d'accueil
│   ├── MedecinDashboardController.cs # Dashboard Médecin
│   ├── PatientDashboardController.cs # Dashboard Patient
│   ├── ProfileController.cs          # Gestion des profils
│   ├── RoleController.cs             # Contrôleur de base (autorisation)
│   ├── SecretaireDashboardController.cs
│   └── UsersController.cs            # Gestion utilisateurs (Admin)
│
├── Data/
│   └── CabinetMedicalContext.cs      # Context EF Core
│
├── Filters/
│   └── RoleAuthorizeAttribute.cs     # Filtre d'autorisation
│
├── Models/
│   ├── Consultation.cs
│   ├── DossierMedical.cs
│   ├── Facture.cs
│   ├── Medecin.cs
│   ├── Ordonnance.cs
│   ├── OrdonnanceDetail.cs
│   ├── Paiement.cs
│   ├── Patient.cs
│   ├── RendezVous.cs
│   ├── Secretaire.cs
│   └── User.cs
│
├── Migrations/
│   └── 20251213221652_InitialCreate.cs
│
├── Views/
│   ├── Account/
│   ├── AdminDashboard/
│   ├── Dashboard/
│   ├── Home/
│   ├── Profile/
│   ├── Users/
│   └── Shared/
│
└── wwwroot/
    ├── css/
    ├── js/
    └── lib/
```

---

## 🚀 INSTRUCTIONS D'UTILISATION

### Prérequis
- Visual Studio 2022 ou supérieur
- .NET 10.0 SDK
- SQL Server (LocalDB ou Express)
- SQL Server Management Studio (optionnel)

### Installation

1. **Cloner le projet**
   ```bash
   git clone [repository-url]
   cd "Cabinet Medical"
   ```

2. **Configurer la base de données**
   - Modifier `appsettings.json` avec votre chaîne de connexion SQL Server
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=VOTRE_SERVEUR;Database=CabinetMedicalDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Appliquer les migrations**
   ```bash
   dotnet ef database update
   ```

4. **Lancer l'application**
   ```bash
   dotnet run
   ```

5. **Accéder à l'application**
   - Ouvrir le navigateur sur `https://localhost:5001` ou `http://localhost:5000`

### Comptes de Test (à créer via l'interface)

Pour tester, créer un utilisateur Admin via la base de données ou l'interface, puis utiliser les fonctionnalités d'ajout pour créer d'autres utilisateurs.

---

## 📝 NOTES IMPORTANTES

### Sécurité Actuelle
⚠️ **IMPORTANT** : Les mots de passe sont actuellement stockés en clair. Il est **fortement recommandé** d'implémenter le hachage des mots de passe (BCrypt ou PBKDF2) avant un déploiement en production.

### Configuration de la Base de Données
- Les contraintes de clés étrangères utilisent `DeleteBehavior.NoAction` pour préserver l'intégrité des données historiques.
- Toutes les suppressions doivent être gérées manuellement avec cascade contrôlée.

### Améliorations Futures
- Migration vers ASP.NET Core Identity pour une sécurité renforcée
- Implémentation d'une API REST si nécessaire
- Ajout de tests unitaires et d'intégration
- Système de notifications en temps réel

---

## 📞 SUPPORT & CONTACT

Pour toute question ou suggestion d'amélioration, n'hésitez pas à contacter l'équipe de développement.

---

**Version du document :** 1.2  
**Dernière mise à jour :** Décembre 2024  
**Auteur :** Équipe de développement Cabinet Medical

