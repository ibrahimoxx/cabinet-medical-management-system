# 💊 RÉSUMÉ : SYSTÈME ORDONNANCE

**Ce qu'on fait :** Création ordonnances avec plusieurs détails (Médecin), consultation (Médecin/Patient), modification, impression.

**Ce qu'on utilise :**
- Modèles `Ordonnance` (ConsultationId) + `OrdonnanceDetail` (Type, Description, Dosage)
- Modification : Suppression/recréation de tous les détails
- Types : "Medicament", "Analyse", "Radiologie"

**Où c'est :**
- `Models/Ordonnance.cs`, `OrdonnanceDetail.cs`
- `Controllers/OrdonnancesController.cs` (Create, Edit, MesOrdonnances, Imprimer)
- `Views/Ordonnances/Create.cshtml`, `Edit.cshtml`, `Index.cshtml`, `Imprimer.cshtml`

**Flux principal :**
1. Médecin crée depuis consultation → Ajoute détails dynamiquement
2. Ordonnance sauvegardée avec tous ses détails
3. Modification → Supprime tous détails + recrée ceux du formulaire
4. Impression → Format professionnel avec tableau détails

