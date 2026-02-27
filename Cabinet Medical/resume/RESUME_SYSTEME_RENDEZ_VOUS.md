# 📅 RÉSUMÉ : SYSTÈME RENDEZ-VOUS

**Ce qu'on fait :** Planification RDV (Secrétaire/Patient), consultation filtrée, modification (Secrétaire), annulation avec alertes, calcul disponibilité en temps réel.

**Ce qu'on utilise :**
- Modèle `RendezVous` (PatientId, MedecinId, DateRdv, HeureRdv, Statut)
- API `GetAvailableSlots()` (créneaux disponibles)
- Validation : Lundi-Vendredi, 08:00-17:00, créneaux 30 min
- JavaScript (chargement dynamique créneaux)

**Où c'est :**
- `Models/RendezVous.cs`
- `Controllers/RendezVousController.cs` (Create, Edit, Annuler, GetAvailableSlots, ValidateAppointment)
- `Views/RendezVous/Index.cshtml`, `Create.cshtml`, `MesRendezVous.cshtml`

**Flux principal :**
1. Sélection Patient/Médecin/Date → API calcule créneaux disponibles
2. Validation : jour ouvrable, heure valide, pas de doublon
3. Création RDV → Statut "Planifie"
4. Annulation → Alerte créée + Statut "Annule"

