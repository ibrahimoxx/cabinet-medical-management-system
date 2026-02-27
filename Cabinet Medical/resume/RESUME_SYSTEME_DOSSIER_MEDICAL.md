# 📁 RÉSUMÉ : SYSTÈME DOSSIER MÉDICAL

**Ce qu'on fait :** Création dossiers (Secrétaire), consultation historique complet (tous rôles), modification remarques (Secrétaire).

**Ce qu'on utilise :**
- Modèle `DossierMedical` (PatientId unique, Remarques)
- Relation One-to-One avec Patient
- Relation One-to-Many avec Consultations
- Filtre : nom patient

**Où c'est :**
- `Models/DossierMedical.cs`
- `Controllers/DossierMedicalsController.cs` (Create, Index, Consulter, MonDossier, Edit)
- `Views/DossierMedicals/Index.cshtml`, `Consulter.cshtml`, `MonDossier.cshtml`

**Flux principal :**
1. Secrétaire crée dossier → Un patient = un dossier unique
2. Consultations créées → Liées au dossier
3. Consultation dossier → Historique complet (consultations + ordonnances)
4. Patient consulte → Voit tout son historique médical

