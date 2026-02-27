# 💊 EXPLICATION DU SYSTÈME DE GESTION DES ORDONNANCES

## 📋 Vue d'ensemble

Le système de gestion des ordonnances permet de :
1. **Créer des ordonnances** avec plusieurs détails (Médecin uniquement)
2. **Consulter les ordonnances** (Médecin et Patient)
3. **Modifier des ordonnances** avec mise à jour des détails (Médecin uniquement)
4. **Supprimer des ordonnances** (Médecin uniquement)
5. **Imprimer des ordonnances** (tous les rôles autorisés)

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données**

#### `Ordonnance.cs`
**Champs importants :**
- `ConsultationId` : ID de la consultation associée (obligatoire)
- `DateOrdonnance` : Date de l'ordonnance (par défaut = maintenant)

**Relations :**
- Relation Many-to-One avec `Consultation`
- Relation One-to-Many avec `OrdonnanceDetail` (une ordonnance peut avoir plusieurs détails)

#### `OrdonnanceDetail.cs`
**Champs importants :**
- `OrdonnanceId` : ID de l'ordonnance parente (obligatoire)
- `Type` : Type d'élément ("Medicament", "Analyse", "Radiologie")
- `Description` : Description détaillée (obligatoire)
- `Dosage` : Dosage ou instructions (optionnel)

**Localisation :** `Models/Ordonnance.cs` et `Models/OrdonnanceDetail.cs`

**Point important :** Une ordonnance est créée depuis une consultation. Une consultation peut avoir plusieurs ordonnances (suivi, modifications).

---

## 🔄 FLUX 1 : CRÉER UNE ORDONNANCE

### Depuis une consultation

```
┌─────────────────────────────────────────────┐
│ Médecin consulte une Consultation           │
│ Clique "Créer ordonnance"                   │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Ordonnances/Create?consultationId=X
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Create() [GET]        │
│                                             │
│ 1. Vérifie le rôle (doit être Medecin)     │
│                                             │
│ 2. Récupère la Consultation :              │
│    → Consultation → DossierMedical         │
│    → Consultation → Medecin                │
│    → Consultation → Patient                │
│                                             │
│ 3. Pré-remplit ConsultationId              │
│                                             │
│ 4. Affiche le formulaire avec :            │
│    → Informations consultation             │
│    → Formulaire dynamique pour ajouter     │
│       plusieurs détails                     │
└─────────────┬───────────────────────────────┘
              │
              │ Médecin ajoute les détails :
              │ - Type (Médicament/Analyse/Radiologie)
              │ - Description
              │ - Dosage (optionnel)
              │
              │ (+ Bouton pour ajouter d'autres détails)
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Create() [POST]       │
│                                             │
│ 1. VALIDATION :                            │
│    → ConsultationId valide ?               │
│    → Au moins un détail présent ?          │
│    → Tous les détails ont Type et          │
│       Description ?                         │
│                                             │
│ 2. CRÉATION :                              │
│    → Crée l'Ordonnance                     │
│    → Pour chaque détail soumis :           │
│       Crée un OrdonnanceDetail             │
│       Lie à l'ordonnance                   │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/OrdonnancesController.cs` - Méthode `Create()` (GET et POST)
- Frontend : `Views/Ordonnances/Create.cshtml`

**Fonctionnalité spéciale :** Le formulaire permet d'ajouter plusieurs lignes de détails dynamiquement via JavaScript. Chaque ligne peut contenir un médicament, une analyse, ou une radiologie.

---

## 🔄 FLUX 2 : CONSULTER LES ORDONNANCES

### Vue Médecin : Ses ordonnances uniquement

```
┌─────────────────────────────────────────────┐
│ Médecin accède à /Ordonnances/Index         │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Ordonnances/Index
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Index()               │
│                                             │
│ 1. Vérifie le rôle (doit être Medecin)     │
│                                             │
│ 2. Récupère le Medecin connecté            │
│                                             │
│ 3. Filtre les ordonnances :                │
│    → Par MedecinId (via Consultation)      │
│    → N'affiche que SES ordonnances         │
│                                             │
│ 4. FILTRES DISPONIBLES :                   │
│    → Par nom de patient                    │
│    → Par date                              │
│                                             │
│ 5. Inclut les relations :                  │
│    → Consultation → DossierMedical         │
│    → Consultation → Patient → User         │
│    → OrdonnanceDetails (tous)              │
│                                             │
│ 6. Tri par date décroissante               │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Patient                                   │
│ - Date                                      │
│ - Nombre de détails                         │
│ - Actions :                                 │
│   → Détails                                 │
│   → Modifier                                │
│   → Supprimer                               │
│   → Imprimer                                │
└─────────────────────────────────────────────┘
```

### Vue Patient : Mes ordonnances

```
┌─────────────────────────────────────────────┐
│ Patient accède à /Ordonnances/MesOrdonnances│
└─────────────┬───────────────────────────────┘
              │
              │ GET /Ordonnances/MesOrdonnances
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.MesOrdonnances()      │
│                                             │
│ 1. Vérifie le rôle (doit être Patient)     │
│                                             │
│ 2. Récupère le Patient connecté            │
│                                             │
│ 3. Récupère son DossierMedical             │
│                                             │
│ 4. Filtre les ordonnances :                │
│    → Par DossierMedicalId                  │
│    → Toutes les ordonnances de TOUTES      │
│       ses consultations                     │
│                                             │
│ 5. Inclut les relations :                  │
│    → Consultation → Medecin                │
│    → OrdonnanceDetails (tous)              │
│                                             │
│ 6. Tri par date décroissante               │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/OrdonnancesController.cs` - Méthodes `Index()` et `MesOrdonnances()`
- Frontend : `Views/Ordonnances/Index.cshtml` et `Views/Ordonnances/MesOrdonnances.cshtml`

---

## 🔄 FLUX 3 : VOIR LES DÉTAILS D'UNE ORDONNANCE

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Détails" sur une       │
│ ordonnance                                  │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Ordonnances/Details/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Details()             │
│                                             │
│ 1. Récupère l'Ordonnance par ID            │
│    → Inclut Consultation → Patient         │
│    → Inclut Consultation → Medecin         │
│    → Inclut TOUS les OrdonnanceDetails     │
│                                             │
│ 2. Affiche toutes les informations :       │
│    → Informations patient                   │
│    → Date                                   │
│    → Médecin prescripteur                   │
│    → Liste complète des détails :          │
│       - Type (badge coloré)                 │
│       - Description                         │
│       - Dosage                              │
│                                             │
│ 3. Actions disponibles :                   │
│    → Modifier (Médecin)                     │
│    → Supprimer (Médecin)                    │
│    → Imprimer                               │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/OrdonnancesController.cs` - Méthode `Details()`
- Frontend : `Views/Ordonnances/Details.cshtml`

---

## 🔄 FLUX 4 : MODIFIER UNE ORDONNANCE (MÉDECIN)

### Modification avec mise à jour des détails

```
┌─────────────────────────────────────────────┐
│ Médecin clique "Modifier" sur une          │
│ ordonnance                                  │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Ordonnances/Edit/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Edit() [GET]          │
│                                             │
│ 1. Vérifie le rôle (doit être Medecin)     │
│                                             │
│ 2. Récupère l'Ordonnance                   │
│    → Inclut tous les OrdonnanceDetails     │
│                                             │
│ 3. Affiche le formulaire pré-rempli        │
│    → Tous les détails existants affichés   │
│    → Possibilité d'ajouter/supprimer       │
└─────────────┬───────────────────────────────┘
              │
              │ Médecin modifie et soumet
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Edit() [POST]         │
│                                             │
│ 1. VALIDATION :                            │
│    → Au moins un détail présent ?          │
│    → Tous les détails ont Type et          │
│       Description ?                         │
│                                             │
│ 2. MODIFICATION STRATÉGIQUE :              │
│    ⚠️ Supprime TOUS les OrdonnanceDetails │
│       existants                             │
│    → Recrée TOUS les détails soumis        │
│    → Cette méthode garantit une mise à     │
│       jour complète                         │
│                                             │
│ 3. SAUVEGARDE :                            │
│    → Mise à jour de l'Ordonnance           │
│    → Sauvegarde des nouveaux détails       │
│                                             │
│ 4. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Pourquoi supprimer et recréer les détails ?**
- Permet d'ajouter/supprimer/modifier facilement
- Évite la gestion complexe de chaque détail individuellement
- Garantit que la BDD reflète exactement le formulaire soumis

**Localisation :**
- Backend : `Controllers/OrdonnancesController.cs` - Méthode `Edit()` (GET et POST)
- Frontend : `Views/Ordonnances/Edit.cshtml`

---

## 🔄 FLUX 5 : IMPRIMER UNE ORDONNANCE

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Imprimer" sur une      │
│ ordonnance                                  │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Ordonnances/Imprimer/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ OrdonnancesController.Imprimer()            │
│                                             │
│ 1. Récupère l'Ordonnance                   │
│    → Inclut toutes les relations           │
│    → Inclut TOUS les OrdonnanceDetails     │
│                                             │
│ 2. Retourne une vue spéciale :             │
│    → Layout = null (pas de menu)           │
│    → Style optimisé pour impression        │
│    → Auto-impression via JavaScript        │
│                                             │
│ 3. Contenu affiché :                       │
│    → En-tête "ORDONNANCE MÉDICALE"         │
│    → Informations patient                   │
│    → Date                                   │
│    → Médecin prescripteur                   │
│    → Tableau des détails :                 │
│       - Type (Médicament/Analyse/Radiologie)│
│       - Description                         │
│       - Dosage                              │
│    → Signature du médecin (zone)           │
│    → Pied de page (date génération)        │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/OrdonnancesController.cs` - Méthode `Imprimer()`
- Frontend : `Views/Ordonnances/Imprimer.cshtml`

**Fonctionnalités d'impression :**
- Format professionnel type ordonnance médicale
- Tableau structuré pour les détails
- Zone de signature pour le médecin
- Auto-impression au chargement

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`OrdonnancesController.cs`**
   - `Index()` : Liste des ordonnances du médecin (avec filtres)
   - `MesOrdonnances()` : Liste des ordonnances du patient
   - `Create()` (GET/POST) : Créer une ordonnance avec détails
   - `Edit()` (GET/POST) : Modifier une ordonnance (suppression/recréation des détails)
   - `Details()` : Détails d'une ordonnance
   - `Delete()` (GET/POST) : Supprimer une ordonnance
   - `Imprimer()` : Vue d'impression

### Vues

1. **`Views/Ordonnances/Index.cshtml`**
   - Liste pour Médecin avec filtres

2. **`Views/Ordonnances/MesOrdonnances.cshtml`**
   - Liste pour Patient

3. **`Views/Ordonnances/Create.cshtml`**
   - Formulaire dynamique avec ajout/suppression de détails

4. **`Views/Ordonnances/Edit.cshtml`**
   - Formulaire de modification avec détails existants

5. **`Views/Ordonnances/Details.cshtml`**
   - Affichage détaillé avec tous les détails

6. **`Views/Ordonnances/Imprimer.cshtml`**
   - Vue optimisée pour impression

### JavaScript

**Dans Create.cshtml et Edit.cshtml :**
- Ajout dynamique de lignes de détails
- Suppression de lignes
- Validation côté client (tous les champs requis)

---

## 📊 FLUX COMPLET : CRÉER ET UTILISER UNE ORDONNANCE

```
1. Médecin consulte une Consultation
   ↓
2. Clique "Créer ordonnance"
   ↓
3. Formulaire s'affiche avec Consultation pré-remplie
   ↓
4. Médecin ajoute des détails :
   → Ligne 1 : Médicament + Description + Dosage
   → Ligne 2 : Analyse + Description
   → Ligne 3 : Radiologie + Description
   (peut en ajouter plus)
   ↓
5. Soumet le formulaire
   ↓
6. Validation (au moins 1 détail, Type et Description requis)
   ↓
7. Création en BDD :
   → Ordonnance créée
   → Tous les OrdonnanceDetails créés
   ↓
8. Ordonnance visible dans :
   - Index des ordonnances (Médecin)
   - Mes ordonnances (Patient)
   - Détails de la consultation
   ↓
9. Actions possibles :
   → Voir détails
   → Modifier (Médecin)
   → Supprimer (Médecin)
   → Imprimer (pour le patient ou le médecin)
```

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Flexibilité** : Plusieurs détails par ordonnance
- **Types variés** : Médicaments, analyses, radiologies
- **Modification simple** : Suppression/recréation des détails
- **Impression professionnelle** : Format standard
- **Sécurité** : Seul le médecin peut créer/modifier/supprimer

### ⚠️ Limitations actuelles

- Pas de base de données de médicaments (saisie libre)
- Pas de validation des dosages
- Pas de durée de traitement
- Pas de renouvellement automatique

---

## 📝 CONCLUSION

Le système de gestion des ordonnances est **entièrement fonctionnel** :
- ✅ Création avec plusieurs détails (Médecin uniquement)
- ✅ Consultation par Médecin et Patient
- ✅ Modification avec mise à jour complète des détails
- ✅ Suppression sécurisée
- ✅ Impression professionnelle
- ✅ Intégration avec consultations

**Le système permet au médecin de créer des ordonnances complètes avec plusieurs éléments (médicaments, analyses, radiologies) pour chaque consultation.**

