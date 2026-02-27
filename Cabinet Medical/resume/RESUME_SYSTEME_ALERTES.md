# 🔔 RÉSUMÉ : SYSTÈME D'ALERTES

**Ce qu'on fait :** Notifie les utilisateurs en temps réel des annulations de RDV et des rappels 24h avant.

**Ce qu'on utilise :**
- Modèle `Alerte` (Type, Message, UserId, EstLue)
- API `GetUnreadCount()` (badge notification)
- JavaScript (actualisation toutes les 30s)

**Où c'est :**
- `Models/Alerte.cs`
- `Controllers/AlertesController.cs`
- `Controllers/RendezVousController.cs` (CreateAlerteAnnulation, CheckRappels)
- `Views/Alertes/Index.cshtml`
- `Views/Shared/_Layout.cshtml` (badge + JavaScript)

**Flux principal :**
1. Annulation RDV → Alerte créée (Patient ↔ Secrétaire/Médecin)
2. 24h avant RDV → Rappel automatique (Patient)
3. Badge rouge mis à jour toutes les 30 secondes
4. Utilisateur consulte et marque comme lue

