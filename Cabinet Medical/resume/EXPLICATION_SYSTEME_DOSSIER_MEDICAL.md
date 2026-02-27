# 📁 EXPLICATION DU SYSTÈME DE GESTION DES DOSSIERS MÉDICAUX

## 📋 Vue d'ensemble

Le système de gestion des dossiers médicaux permet de :
1. **Créer des dossiers médicaux** (Secrétaire uniquement)
2. **Consulter les dossiers** (Secrétaire et Médecin voient tous, Patient voit le sien)
3. **Modifier les dossiers** (Secrétaire uniquement)
4. **Accéder à l'historique complet** : consultations, ordonnances liées au dossier

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données (`DossierMedical.cs`)**

**Champs importants :**
- `PatientId` : ID du patient (obligatoire, unique - un patient = un dossier)
- `DateCreation` : Date de création (par défaut = maintenant)
- `Remarques` : Notes générales sur le dossier (texte libre, optionnel)

**Relations :**
- Relation One-to-One avec `Patient` (un patient a un seul dossier médical)
- Relation One-to-Many avec `Consultation` (un dossier peut avoir plusieurs consultations)

**Localisation :** `Models/DossierMedical.cs`

**Point important :** Chaque patient a UN SEUL dossier médical. Le dossier médical est le conteneur principal qui regroupe toutes les consultations d'un patient.

---

## 🔄 FLUX 1 : CRÉER UN DOSSIER MÉDICAL (SECRÉTAIRE)

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /DossierMedicals/Create │
└─────────────┬───────────────────────────────┘
              │
              │ GET /DossierMedicals/Create
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.Create() [GET]    │
│                                             │
│ 1. Vérifie le rôle (doit être Secretaire)  │
│                                             │
│ 2. Charge la liste des patients :          │
│    → Patients qui N'ONT PAS encore de      │
│       dossier médical                       │
│    → Avec User (actifs uniquement)         │
│                                             │
│ 3. Affiche le formulaire                   │
└─────────────┬───────────────────────────────┘
              │
              │ Utilisateur sélectionne :
              │ - Patient (dans la liste)
              │ - Remarques (optionnel)
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.Create() [POST]   │
│                                             │
│ 1. VALIDATION :                            │
│    → PatientId valide ?                    │
│    → Patient n'a pas déjà de dossier ?     │
│                                             │
│ 2. CRÉATION :                              │
│    → Crée le DossierMedical                │
│    → DateCreation = maintenant             │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/DossierMedicalsController.cs` - Méthode `Create()` (GET et POST)
- Frontend : `Views/DossierMedicals/Create.cshtml`

**Validation importante :** Le système vérifie qu'un patient n'a pas déjà de dossier médical avant de créer un nouveau (relation One-to-One).

---

## 🔄 FLUX 2 : CONSULTER LES DOSSIERS MÉDICAUX

### Vue Secrétaire et Médecin : Tous les dossiers

```
┌─────────────────────────────────────────────┐
│ Utilisateur accède à /DossierMedicals/Index │
└─────────────┬───────────────────────────────┘
              │
              │ GET /DossierMedicals/Index
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.Index()            │
│                                             │
│ 1. Vérifie le rôle (Secretaire ou Medecin) │
│                                             │
│ 2. FILTRES DISPONIBLES :                   │
│    → Par nom de patient                    │
│                                             │
│ 3. Récupère TOUS les dossiers :            │
│    → Inclut Patient → User                 │
│    → Tri par date de création              │
│                                             │
│ 4. Affiche la liste                        │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Patient (Nom, Prénom)                     │
│ - Date de création                          │
│ - Actions :                                 │
│   → Consulter (détails complets)            │
│   → Modifier (Secrétaire uniquement)        │
└─────────────────────────────────────────────┘
```

### Vue Patient : Mon dossier médical

```
┌─────────────────────────────────────────────┐
│ Patient accède à /DossierMedicals/MonDossier│
└─────────────┬───────────────────────────────┘
              │
              │ GET /DossierMedicals/MonDossier
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.MonDossier()       │
│                                             │
│ 1. Vérifie le rôle (doit être Patient)     │
│                                             │
│ 2. Récupère le Patient connecté            │
│                                             │
│ 3. Récupère son DossierMedical             │
│    → Si pas de dossier :                   │
│       Message "Aucun dossier médical"      │
│                                             │
│ 4. Inclut TOUTES les Consultations         │
│    → Avec Médecin                           │
│    → Avec Ordonnances                       │
│    → Triées par date décroissante          │
│                                             │
│ 5. Affiche le dossier complet              │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/DossierMedicalsController.cs` - Méthodes `Index()` et `MonDossier()`
- Frontend : `Views/DossierMedicals/Index.cshtml` et `Views/DossierMedicals/MonDossier.cshtml`

---

## 🔄 FLUX 3 : CONSULTER UN DOSSIER EN DÉTAIL (SECRÉTAIRE/MÉDECIN)

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Consulter" sur un      │
│ dossier médical                             │
└─────────────┬───────────────────────────────┘
              │
              │ GET /DossierMedicals/Consulter/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.Consulter()       │
│                                             │
│ 1. Vérifie le rôle (Secretaire ou Medecin) │
│                                             │
│ 2. Récupère le DossierMedical par ID       │
│    → Inclut Patient → User                 │
│                                             │
│ 3. Charge TOUTES les Consultations :       │
│    → Avec Medecin → User                   │
│    → Avec Ordonnances (si existent)        │
│    → Triées par date décroissante          │
│                                             │
│ 4. Affiche :                               │
│    → Informations patient                   │
│    → Informations dossier                   │
│    → Liste complète des consultations :    │
│       - Date                                │
│       - Médecin                             │
│       - Diagnostic                          │
│       - Actions : Créer ordonnance,        │
│         Voir détails                        │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/DossierMedicalsController.cs` - Méthode `Consulter()`
- Frontend : `Views/DossierMedicals/Consulter.cshtml`

**Fonctionnalité importante :** Depuis cette vue, le Médecin peut créer une nouvelle consultation ou une ordonnance directement, car le `dossierMedicalId` est déjà connu.

---

## 🔄 FLUX 4 : MODIFIER UN DOSSIER MÉDICAL (SECRÉTAIRE)

```
┌─────────────────────────────────────────────┐
│ Secrétaire clique "Modifier" sur un        │
│ dossier médical                             │
└─────────────┬───────────────────────────────┘
              │
              │ GET /DossierMedicals/Edit/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.Edit() [GET]      │
│                                             │
│ 1. Vérifie le rôle (doit être Secretaire)  │
│                                             │
│ 2. Récupère le DossierMedical              │
│                                             │
│ 3. Affiche le formulaire pré-rempli        │
│    → Patient (affiché mais non modifiable) │
│    → Date de création (affichée)           │
│    → Remarques (modifiable)                 │
└─────────────┬───────────────────────────────┘
              │
              │ Secrétaire modifie les remarques
              │
              ▼
┌─────────────────────────────────────────────┐
│ DossierMedicalsController.Edit() [POST]     │
│                                             │
│ 1. Validation                              │
│                                             │
│ 2. Mise à jour en BDD                      │
│    → Seulement les Remarques peuvent être  │
│       modifiées                             │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/DossierMedicalsController.cs` - Méthode `Edit()` (GET et POST)
- Frontend : `Views/DossierMedicals/Edit.cshtml`

**Limitation :** Le patient et la date de création ne peuvent pas être modifiés après création (logique métier).

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`DossierMedicalsController.cs`**
   - `Index()` : Liste tous les dossiers (avec filtre par nom patient)
   - `MonDossier()` : Dossier du patient connecté
   - `Consulter()` : Vue détaillée d'un dossier (Secrétaire/Médecin)
   - `Create()` (GET/POST) : Créer un dossier
   - `Edit()` (GET/POST) : Modifier un dossier (Secrétaire)

### Vues

1. **`Views/DossierMedicals/Index.cshtml`**
   - Liste avec filtre pour Secrétaire/Médecin

2. **`Views/DossierMedicals/MonDossier.cshtml`**
   - Vue patient avec historique complet

3. **`Views/DossierMedicals/Consulter.cshtml`**
   - Vue détaillée avec toutes les consultations

4. **`Views/DossierMedicals/Create.cshtml`**
   - Formulaire de création

5. **`Views/DossierMedicals/Edit.cshtml`**
   - Formulaire de modification

### Relations avec autres systèmes

1. **Patient**
   - Relation One-to-One (un patient = un dossier)

2. **Consultations**
   - Relation One-to-Many (un dossier = plusieurs consultations)
   - Toutes les consultations d'un patient sont regroupées dans son dossier

3. **Ordonnances**
   - Accès indirect via les consultations
   - Une consultation peut avoir plusieurs ordonnances

---

## 📊 FLUX COMPLET : CYCLE DE VIE D'UN DOSSIER MÉDICAL

```
1. Patient créé par Secrétaire
   ↓
2. Secrétaire crée un dossier médical pour ce patient
   ↓
3. Dossier médical créé en BDD
   ↓
4. Médecin crée une consultation
   → Lie au dossier médical
   ↓
5. Consultation visible dans le dossier
   ↓
6. Médecin peut créer des ordonnances
   → Liées à la consultation
   → Visibles dans le dossier
   ↓
7. Plusieurs consultations au fil du temps
   → Toutes visibles dans le dossier
   ↓
8. Patient consulte son dossier
   → Voit tout son historique
   → Consultations, ordonnances, etc.
```

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Historique complet** : Toutes les consultations d'un patient regroupées
- **Sécurité** : Patient ne voit que son dossier
- **Organisation** : Un seul point d'accès pour l'historique médical
- **Traçabilité** : Date de création enregistrée
- **Flexibilité** : Remarques modifiables par la secrétaire

### ⚠️ Limitations actuelles

- Pas de documents joints (scans, images)
- Pas de catégorisation des consultations
- Pas de recherche avancée dans le contenu
- Pas de versionning (historique des modifications)

---

## 📝 CONCLUSION

Le système de gestion des dossiers médicaux est **entièrement fonctionnel** :
- ✅ Création par Secrétaire
- ✅ Consultation différenciée selon le rôle
- ✅ Modification des remarques
- ✅ Vue complète avec historique des consultations
- ✅ Intégration avec consultations et ordonnances

**Le système garantit qu'un patient a un seul dossier médical qui regroupe tout son historique médical dans le cabinet.**

