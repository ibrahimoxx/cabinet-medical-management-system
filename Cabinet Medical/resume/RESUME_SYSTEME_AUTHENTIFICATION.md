# 🔐 RÉSUMÉ : SYSTÈME D'AUTHENTIFICATION

**Ce qu'on fait :** Connexion, inscription patients, gestion sessions, contrôle d'accès par rôle (Admin, Secretaire, Medecin, Patient).

**Ce qu'on utilise :**
- Modèle `User` (Username, PasswordHash, Role, IsActive)
- Sessions HTTP (stockage Role, Username)
- Classe `RoleController` (vérification automatique)
- Protection JavaScript globale dans layout

**Où c'est :**
- `Models/User.cs`
- `Controllers/AccountController.cs` (Login, Register, Logout)
- `Controllers/RoleController.cs` (classe de base)
- `Controllers/DashboardController.cs` (routage selon rôle)
- `Views/Account/Login.cshtml`, `Register.cshtml`
- `Views/Shared/_Layout.cshtml` (protection + menu différentiel)

**Flux principal :**
1. Utilisateur se connecte → Session créée (Role stocké)
2. Redirection vers Dashboard → Routage automatique selon rôle
3. Chaque contrôleur vérifie le rôle avant autorisation
4. Déconnexion → Session supprimée

