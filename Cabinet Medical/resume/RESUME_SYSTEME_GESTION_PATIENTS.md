# 👥 RÉSUMÉ : SYSTÈME GESTION PATIENTS

**Ce qu'on fait :** Création patients avec User (Secrétaire), liste avec recherche multi-critères, modification, suppression, création depuis formulaire RDV.

**Ce qu'on utilise :**
- Modèles `Patient` (Nom, Prenom, UserId) + `User` (création automatique)
- Recherche : nom, prénom, email, téléphone
- ReturnUrl : retour automatique après création

**Où c'est :**
- `Models/Patient.cs`
- `Controllers/PatientsController.cs` (Create, Index, Edit, Delete)
- `Views/Patients/Create.cshtml`, `Index.cshtml`, `Edit.cshtml`

**Flux principal :**
1. Secrétaire crée patient → User créé automatiquement (Role "Patient")
2. Liste patients → Recherche multi-critères
3. Depuis formulaire RDV → Création patient → Retour avec patient pré-sélectionné
4. Modification/Suppression → Sécurisé avec confirmation

