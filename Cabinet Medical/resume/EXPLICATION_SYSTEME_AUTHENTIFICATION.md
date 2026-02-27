# 🔐 EXPLICATION DU SYSTÈME D'AUTHENTIFICATION ET D'AUTORISATION

## 📋 Vue d'ensemble

Le système d'authentification et d'autorisation permet de :
1. **Authentifier les utilisateurs** (connexion avec username/password)
2. **Gérer les sessions** (maintien de l'état connecté)
3. **Contrôler l'accès** selon les rôles (Admin, Secretaire, Medecin, Patient)
4. **Protéger les routes** contre les accès non autorisés

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données (`User.cs`)**

**Champs importants :**
- `Username` : Nom d'utilisateur unique (maximum 50 caractères)
- `PasswordHash` : Mot de passe (stocké en clair actuellement, à hasher en production)
- `Email` : Email unique de l'utilisateur
- `Role` : Rôle de l'utilisateur ("Admin", "Secretaire", "Medecin", "Patient")
- `IsActive` : Statut actif/inactif (permet de désactiver un compte sans le supprimer)
- `CreatedAt` : Date de création du compte

**Localisation :** `Models/User.cs`

---

## 🔄 FLUX 1 : CONNEXION (LOGIN)

### Comment ça fonctionne ?

```
┌─────────────────────────────────────────────┐
│ Utilisateur accède à /Account/Login         │
│ (Page de connexion)                         │
└─────────────┬───────────────────────────────┘
              │
              │ 1. Formulaire rempli
              │    - Username
              │    - Password
              │
              ▼
┌─────────────────────────────────────────────┐
│ AccountController.Login() [POST]            │
│                                             │
│ 1. Recherche l'utilisateur dans la BDD     │
│    - Username correspondant                │
│    - Password correspondant                │
│    - IsActive = true                       │
│                                             │
│ 2. Si trouvé :                             │
│    → Crée une session                      │
│    → Stocke le Role dans la session        │
│    → Stocke le Username dans la session    │
│    → Redirige vers Dashboard               │
│                                             │
│ 3. Si non trouvé :                         │
│    → Affiche message d'erreur              │
│    → Retourne à la page de connexion       │
└─────────────┬───────────────────────────────┘
              │
              │ Session créée
              │
              ▼
┌─────────────────────────────────────────────┐
│ Redirection vers DashboardController.Index()│
│                                             │
│ → Vérifie le rôle dans la session          │
│ → Redirige vers le bon dashboard :         │
│   - Admin → /Dashboard/Admin               │
│   - Secretaire → /Dashboard/Secretaire     │
│   - Medecin → /Dashboard/Medecin           │
│   - Patient → /Dashboard/Patient           │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/AccountController.cs` - Méthode `Login()` (GET et POST)
- Frontend : `Views/Account/Login.cshtml`

---

## 🔄 FLUX 2 : INSCRIPTION (REGISTER) - PATIENTS UNIQUEMENT

### Comment ça fonctionne ?

```
┌─────────────────────────────────────────────┐
│ Patient accède à /Account/Register          │
│ (Formulaire d'inscription)                  │
└─────────────┬───────────────────────────────┘
              │
              │ 1. Formulaire rempli
              │    - Username (unique)
              │    - Email (unique)
              │    - Password
              │    - Nom, Prénom
              │    - Téléphone, Adresse
              │    - Date de naissance
              │    - Antécédents médicaux
              │
              ▼
┌─────────────────────────────────────────────┐
│ AccountController.Register() [POST]         │
│                                             │
│ 1. VALIDATION :                            │
│    → Username unique ?                     │
│    → Email unique ?                        │
│    → Email format valide ?                 │
│    → Tous les champs requis remplis ?      │
│                                             │
│ 2. CRÉATION UTILISATEUR :                  │
│    → Crée un User                          │
│    → Role = "Patient"                      │
│    → IsActive = true                       │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. CRÉATION PATIENT :                      │
│    → Crée un Patient                       │
│    → Lie au User créé (UserId)             │
│    → Sauvegarde en BDD                     │
│                                             │
│ 4. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Login                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/AccountController.cs` - Méthode `Register()` (GET et POST)
- Frontend : `Views/Account/Register.cshtml`

**Note :** Seuls les patients peuvent s'inscrire eux-mêmes. Les autres rôles (Admin, Secretaire, Medecin) doivent être créés par un Admin.

---

## 🔄 FLUX 3 : DÉCONNEXION (LOGOUT)

### Comment ça fonctionne ?

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique sur "Logout" dans le menu│
└─────────────┬───────────────────────────────┘
              │
              │ GET /Account/Logout
              │
              ▼
┌─────────────────────────────────────────────┐
│ AccountController.Logout()                  │
│                                             │
│ 1. Supprime toutes les données de session  │
│    → HttpContext.Session.Clear()           │
│                                             │
│ 2. Redirige vers /Account/Login            │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/AccountController.cs` - Méthode `Logout()`
- Frontend : Lien dans `Views/Shared/_Layout.cshtml`

---

## 🔒 FLUX 4 : PROTECTION DES ROUTES PAR RÔLE

### Mécanisme de protection

Le système utilise **deux méthodes principales** :

#### Méthode 1 : Vérification manuelle dans chaque contrôleur

```
┌─────────────────────────────────────────────┐
│ Dans chaque action de contrôleur :          │
│                                             │
│ var role = HttpContext.Session.GetString(   │
│     "UserRole");                            │
│                                             │
│ if (role != "Secretaire")                   │
│     return RedirectToAction("Login");       │
└─────────────────────────────────────────────┘
```

#### Méthode 2 : Classe de base `RoleController`

```
┌─────────────────────────────────────────────┐
│ Contrôleurs qui héritent de RoleController :│
│                                             │
│ public class PatientsController             │
│     : RoleController                        │
│ {                                           │
│     public PatientsController()             │
│         : base("Secretaire")  // Rôle requis│
│     { }                                     │
│ }                                           │
│                                             │
│ → Vérifie automatiquement le rôle          │
│ → Redirige vers Login si non autorisé      │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Classe de base : `Controllers/Base/RoleController.cs`
- Utilisée par : `PatientsController`, `UsersController`

---

## 🔒 FLUX 5 : PROTECTION GLOBALE PAR SESSION

### Protection au niveau du layout

```
┌─────────────────────────────────────────────┐
│ Dans _Layout.cshtml :                       │
│                                             │
│ JavaScript vérifie à chaque chargement :    │
│                                             │
│ if (Session["UserRole"] == null            │
│     && page != "/Account/Login")           │
│ {                                           │
│     → Redirige vers /Account/Login         │
│ }                                           │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Frontend : `Views/Shared/_Layout.cshtml` (début du fichier)

**Avantage :** Empêche l'accès direct aux pages si la session a expiré ou n'existe pas.

---

## 📊 STRUCTURE DES RÔLES ET PERMISSIONS

### Rôles et leurs accès

| Rôle | Peut accéder à |
|------|----------------|
| **Admin** | Gestion des utilisateurs, Dashboard admin, Profil, Alertes |
| **Secretaire** | Gestion des patients, Rendez-vous, Dossiers médicaux, Factures, Consultations, Profil, Alertes |
| **Medecin** | Mes rendez-vous, Consultations, Ordonnances, Dossiers médicaux, Profil, Alertes |
| **Patient** | Prendre RDV, Mes rendez-vous, Mon dossier médical, Mes factures, Mes ordonnances, Payer factures, Profil, Alertes |

**Contrôle d'accès :**
- Chaque contrôleur vérifie le rôle avant d'autoriser l'accès
- Les vues affichent des menus différents selon le rôle
- Les actions spécifiques vérifient les permissions

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`AccountController.cs`**
   - `Login()` (GET) : Affiche le formulaire de connexion
   - `Login()` (POST) : Traite la connexion et crée la session
   - `Register()` (GET) : Affiche le formulaire d'inscription
   - `Register()` (POST) : Traite l'inscription (création User + Patient)
   - `Logout()` : Déconnecte l'utilisateur et supprime la session

2. **`RoleController.cs`** (classe de base)
   - `OnActionExecuting()` : Vérifie automatiquement le rôle avant chaque action

3. **`DashboardController.cs`**
   - `Index()` : Route vers le bon dashboard selon le rôle
   - `Admin()`, `Secretaire()`, `Medecin()`, `Patient()` : Dashboards spécifiques

### Vues

1. **`Views/Account/Login.cshtml`**
   - Formulaire de connexion avec validation

2. **`Views/Account/Register.cshtml`**
   - Formulaire d'inscription complet pour les patients

3. **`Views/Shared/_Layout.cshtml`**
   - Protection globale par JavaScript
   - Menu différentiel selon le rôle

### Configuration

**Dans `Program.cs` :**
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
app.UseSession();
```

Cela permet d'utiliser les sessions pour stocker les données utilisateur.

---

## 📊 FLUX COMPLET DE CONNEXION

```
1. Utilisateur arrive sur le site
   ↓
2. Vérification de session dans _Layout.cshtml
   ↓
3. Si pas de session → Redirection vers /Account/Login
   ↓
4. Utilisateur saisit username/password
   ↓
5. POST /Account/Login
   ↓
6. Vérification en BDD (Username + Password + IsActive)
   ↓
7. Si valide :
   → Création session (Role, Username)
   → Redirection vers Dashboard
   ↓
8. Dashboard.Index() détecte le rôle
   ↓
9. Redirection vers le dashboard spécifique
   ↓
10. Utilisateur accède aux fonctionnalités selon son rôle
```

---

## ⚙️ SÉCURITÉ ET PROTECTIONS

### Protections implémentées

1. **Validation des données**
   - Vérification username/password en BDD
   - Validation format email
   - Vérification unicité username/email

2. **Gestion des sessions**
   - Stockage sécurisé dans la session serveur
   - Vérification à chaque requête

3. **Protection CSRF**
   - `[ValidateAntiForgeryToken]` sur tous les formulaires POST

4. **Contrôle d'accès**
   - Vérification du rôle à chaque action
   - Redirection automatique si non autorisé

### ⚠️ Améliorations recommandées pour la production

1. **Hashage des mots de passe**
   - Actuellement stockés en clair
   - Utiliser BCrypt ou ASP.NET Identity

2. **Expiration de session**
   - Configurer un timeout de session

3. **Validation renforcée**
   - Limiter les tentatives de connexion
   - Captcha pour l'inscription

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Système simple et efficace** : Sessions en mémoire
- **Contrôle d'accès granulaire** : Vérification par action
- **Interface intuitive** : Connexion/inscription claires
- **Protection globale** : JavaScript dans le layout

### ⚠️ Limitations actuelles

- Mots de passe en clair (à hasher en production)
- Pas de réinitialisation de mot de passe
- Pas de "Se souvenir de moi"
- Sessions en mémoire (perdues au redémarrage du serveur)

---

## 📝 CONCLUSION

Le système d'authentification et d'autorisation est **entièrement fonctionnel** :
- ✅ Connexion sécurisée par session
- ✅ Inscription pour les patients
- ✅ Protection des routes par rôle
- ✅ Déconnexion propre
- ✅ Dashboards différenciés selon le rôle

**Le système garantit que chaque utilisateur accède uniquement aux fonctionnalités autorisées pour son rôle.**

