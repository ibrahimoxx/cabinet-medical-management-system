# 🩺 RÉSUMÉ : SYSTÈME CONSULTATION

**Ce qu'on fait :** Création consultations (Médecin), consultation historique (Médecin/Secrétaire), modification/suppression (Médecin), impression.

**Ce qu'on utilise :**
- Modèle `Consultation` (DossierMedicalId, MedecinId, Diagnostic, Notes)
- Suppression en cascade (Paiements → Factures → OrdonnanceDetails → Ordonnances → Consultation)
- Filtres : nom patient, date

**Où c'est :**
- `Models/Consultation.cs`
- `Controllers/ConsultationsController.cs` (Create, Edit, DeleteConfirmed, Imprimer)
- `Views/Consultations/Index.cshtml`, `Create.cshtml`, `Details.cshtml`, `Imprimer.cshtml`

**Flux principal :**
1. Médecin crée consultation → Lie au dossier médical
2. Consultation visible dans dossier patient + liste médecin
3. Suppression → Cascade automatique (évite erreurs FK)
4. Impression → Format professionnel sans layout

