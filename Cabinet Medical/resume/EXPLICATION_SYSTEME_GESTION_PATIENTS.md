# 👥 EXPLICATION DU SYSTÈME DE GESTION DES PATIENTS

## 📋 Vue d'ensemble

Le système de gestion des patients permet de :
1. **Créer des patients** avec création automatique du compte utilisateur (Secrétaire)
2. **Consulter la liste des patients** avec filtres avancés (Secrétaire)
3. **Modifier les informations** des patients (Secrétaire)
4. **Supprimer des patients** (Secrétaire)
5. **Créer un patient depuis le formulaire de rendez-vous** avec retour automatique

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données (`Patient.cs`)**

**Champs importants :**
- `UserId` : ID de l'utilisateur associé (obligatoire, relation One-to-One)
- `Nom` : Nom du patient (obligatoire, max 50 caractères)
- `Prenom` : Prénom du patient (obligatoire, max 50 caractères)
- `DateNaissance` : Date de naissance (optionnel)
- `Adresse` : Adresse complète (optionnel)
- `Telephone` : Numéro de téléphone (optionnel)
- `AntecedentsMedicaux` : Antécédents médicaux (optionnel)

**Relations :**
- Relation One-to-One avec `User` (un patient = un compte utilisateur)
- Relation One-to-One avec `DossierMedical` (un patient = un dossier médical)

**Localisation :** `Models/Patient.cs`

**Point important :** Un patient est toujours lié à un compte utilisateur. La création d'un patient crée automatiquement un `User` avec le rôle "Patient".

---

## 🔄 FLUX 1 : CRÉER UN PATIENT (SECRÉTAIRE)

### Création standard

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /Patients/Create        │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Patients/Create
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.Create() [GET]           │
│                                             │
│ 1. Vérifie le rôle (via RoleController)    │
│                                             │
│ 2. Affiche le formulaire avec tous les     │
│    champs nécessaires :                     │
│    → Username (pour le compte User)        │
│    → Email                                 │
│    → Password                              │
│    → Nom, Prénom                           │
│    → Date de naissance                     │
│    → Téléphone                             │
│    → Adresse                               │
│    → Antécédents médicaux                  │
└─────────────┬───────────────────────────────┘
              │
              │ Utilisateur remplit et soumet
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.CreatePost() [POST]      │
│                                             │
│ 1. VALIDATION :                            │
│    → Username unique ?                     │
│    → Email unique et format valide ?       │
│    → Password longueur >= 4 caractères ?   │
│    → Nom et Prénom remplis ?               │
│    → Tous les champs requis présents ?     │
│                                             │
│ 2. CRÉATION USER :                         │
│    → Crée un User                          │
│    → Username, Email, PasswordHash         │
│    → Role = "Patient"                      │
│    → IsActive = true                       │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. CRÉATION PATIENT :                      │
│    → Crée un Patient                       │
│    → Lie au User créé (UserId)             │
│    → Toutes les informations personnelles  │
│    → Sauvegarde en BDD                     │
│                                             │
│ 4. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

### Création depuis le formulaire de rendez-vous

**Différence :** Si `returnUrl` est fourni (depuis `/RendezVous/Create`), après création :

```
┌─────────────────────────────────────────────┐
│ Après création réussie :                    │
│                                             │
│ 1. Récupère le returnUrl                   │
│                                             │
│ 2. Construit l'URL de redirection :        │
│    → returnUrl + ?patientId={nouveau_id}  │
│                                             │
│ 3. Redirige vers le formulaire de RDV      │
│                                             │
│ 4. Le formulaire de RDV :                  │
│    → Détecte le patientId dans l'URL       │
│    → Pré-sélectionne le patient créé       │
│    → Affiche message de succès             │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/PatientsController.cs` - Méthodes `Create()` (GET) et `CreatePost()` (POST)
- Frontend : `Views/Patients/Create.cshtml`

**Fonctionnalité spéciale :** Le bouton "Nouveau Patient" dans le formulaire de rendez-vous redirige vers `/Patients/Create` avec un `returnUrl`, permettant de créer un patient et de revenir automatiquement au formulaire de rendez-vous avec le patient pré-sélectionné.

---

## 🔄 FLUX 2 : CONSULTER LA LISTE DES PATIENTS

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /Patients/Index         │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Patients/Index?search=...
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.Index()                  │
│                                             │
│ 1. Vérifie le rôle (via RoleController)    │
│                                             │
│ 2. Récupère tous les patients :            │
│    → Inclut User (pour vérifier IsActive)  │
│    → Inclut DossierMedical (si existe)     │
│                                             │
│ 3. FILTRE DE RECHERCHE :                   │
│    → Si "search" fourni :                  │
│       Recherche dans :                      │
│       - Nom                                 │
│       - Prénom                              │
│       - Email (via User)                    │
│       - Téléphone                           │
│                                             │
│ 4. Tri par nom alphabétique                │
│                                             │
│ 5. Affiche la liste filtrée                │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Nom, Prénom                               │
│ - Email                                     │
│ - Téléphone                                 │
│ - Dossier médical (oui/non)                 │
│ - Actions :                                 │
│   → Détails                                 │
│   → Modifier                                │
│   → Supprimer (avec confirmation)           │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/PatientsController.cs` - Méthode `Index()`
- Frontend : `Views/Patients/Index.cshtml`

**Fonctionnalité de recherche :** Le filtre recherche dans plusieurs champs simultanément (nom, prénom, email, téléphone), facilitant la recherche rapide d'un patient.

---

## 🔄 FLUX 3 : VOIR LES DÉTAILS D'UN PATIENT

```
┌─────────────────────────────────────────────┐
│ Secrétaire clique "Détails" sur un patient  │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Patients/Details/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.Details()                │
│                                             │
│ 1. Récupère le Patient par ID              │
│    → Inclut User                            │
│    → Inclut DossierMedical (si existe)     │
│                                             │
│ 2. Affiche toutes les informations :       │
│    → Informations personnelles              │
│    → Informations du compte User            │
│    → État du dossier médical                │
│    → Actions disponibles                    │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/PatientsController.cs` - Méthode `Details()`
- Frontend : `Views/Patients/Details.cshtml`

---

## 🔄 FLUX 4 : MODIFIER UN PATIENT (SECRÉTAIRE)

```
┌─────────────────────────────────────────────┐
│ Secrétaire clique "Modifier" sur un patient │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Patients/Edit/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.Edit() [GET]             │
│                                             │
│ 1. Récupère le Patient                     │
│                                             │
│ 2. Affiche le formulaire pré-rempli        │
│    → Informations patient                   │
│    → Informations User (readonly)           │
└─────────────┬───────────────────────────────┘
              │
              │ Secrétaire modifie et soumet
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.Edit() [POST]            │
│                                             │
│ 1. Validation                              │
│                                             │
│ 2. Mise à jour en BDD                      │
│    → Met à jour Patient                    │
│    → Optionnellement User (email, etc.)    │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/PatientsController.cs` - Méthode `Edit()` (GET et POST)
- Frontend : `Views/Patients/Edit.cshtml`

---

## 🔄 FLUX 5 : SUPPRIMER UN PATIENT (SECRÉTAIRE)

```
┌─────────────────────────────────────────────┐
│ Secrétaire clique "Supprimer" sur un patient│
└─────────────┬───────────────────────────────┘
              │
              │ 1. Confirmation JavaScript
              │
              ▼
┌─────────────────────────────────────────────┐
│ POST /Patients/Delete/{id}                  │
└─────────────┬───────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────┐
│ PatientsController.DeleteConfirmed()        │
│                                             │
│ ⚠️ ATTENTION : Suppression en cascade     │
│                                             │
│ 1. Supprime le Patient                     │
│                                             │
│ 2. Optionnellement :                       │
│    → Désactive le User (IsActive = false) │
│    → OU supprime le User (selon logique)   │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/PatientsController.cs` - Méthode `DeleteConfirmed()`
- Frontend : `Views/Patients/Delete.cshtml`

**Sécurité :** Une confirmation est demandée avant suppression pour éviter les erreurs.

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`PatientsController.cs`** (hérite de `RoleController`)
   - `Index()` : Liste avec filtre de recherche
   - `Create()` (GET) : Formulaire de création
   - `CreatePost()` (POST) : Création User + Patient
   - `Details()` : Détails d'un patient
   - `Edit()` (GET/POST) : Modifier un patient
   - `Delete()` (GET) : Confirmation
   - `DeleteConfirmed()` (POST) : Suppression

### Vues

1. **`Views/Patients/Index.cshtml`**
   - Liste avec barre de recherche
   - Actions par patient

2. **`Views/Patients/Create.cshtml`**
   - Formulaire complet User + Patient
   - Gestion du returnUrl

3. **`Views/Patients/Details.cshtml`**
   - Affichage détaillé

4. **`Views/Patients/Edit.cshtml`**
   - Formulaire de modification

5. **`Views/Patients/Delete.cshtml`**
   - Page de confirmation

### Intégration avec autres systèmes

1. **Rendez-vous**
   - Possibilité de créer un patient depuis le formulaire de rendez-vous
   - Retour automatique avec patient pré-sélectionné

2. **Dossier médical**
   - Affichage de l'existence du dossier médical dans la liste
   - Lien vers la création du dossier si absent

3. **User**
   - Création automatique du compte utilisateur
   - Synchronisation des informations

---

## 📊 FLUX COMPLET : CRÉER UN PATIENT DEPUIS UN RENDEZ-VOUS

```
1. Secrétaire crée un rendez-vous
   ↓
2. Patient n'existe pas dans la liste
   ↓
3. Clique "Nouveau Patient"
   ↓
4. Redirection vers /Patients/Create?returnUrl=...
   ↓
5. Formulaire de création s'affiche
   ↓
6. Secrétaire remplit toutes les informations
   ↓
7. Soumet le formulaire
   ↓
8. Création User (rôle Patient)
   ↓
9. Création Patient
   ↓
10. Redirection vers returnUrl?patientId={id}
    ↓
11. Formulaire de rendez-vous détecte patientId
    ↓
12. Patient pré-sélectionné automatiquement
    ↓
13. Message "Patient créé avec succès"
    ↓
14. Secrétaire continue la création du RDV
```

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Création en une étape** : User + Patient créés simultanément
- **Intégration fluide** : Retour automatique au formulaire de rendez-vous
- **Recherche avancée** : Filtre multi-critères
- **Sécurité** : Confirmation avant suppression
- **Information complète** : Affichage de l'état du dossier médical

### ⚠️ Limitations actuelles

- Pas de validation du format téléphone
- Pas de photo de profil
- Pas de documents joints (pièce d'identité, etc.)
- Pas d'historique des modifications

---

## 📝 CONCLUSION

Le système de gestion des patients est **entièrement fonctionnel** :
- ✅ Création avec création automatique du compte User
- ✅ Liste avec recherche multi-critères
- ✅ Modification et suppression sécurisées
- ✅ Intégration avec le formulaire de rendez-vous
- ✅ Visualisation de l'état du dossier médical

**Le système permet à la secrétaire de gérer efficacement les patients et facilite la création rapide depuis le formulaire de rendez-vous.**

