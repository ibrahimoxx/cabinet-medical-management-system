# 🩺 EXPLICATION DU SYSTÈME DE GESTION DES CONSULTATIONS

## 📋 Vue d'ensemble

Le système de gestion des consultations permet de :
1. **Créer des consultations** médicales (Médecin uniquement)
2. **Consulter l'historique** des consultations (Médecin et Secrétaire)
3. **Modifier les consultations** (Médecin uniquement)
4. **Supprimer des consultations** avec suppression en cascade (Médecin uniquement)
5. **Imprimer des consultations** (Médecin)

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données (`Consultation.cs`)**

**Champs importants :**
- `DossierMedicalId` : ID du dossier médical (obligatoire)
- `MedecinId` : ID du médecin qui effectue la consultation (obligatoire)
- `DateConsultation` : Date et heure de la consultation (par défaut = maintenant)
- `Diagnostic` : Diagnostic médical (texte libre, optionnel)
- `Notes` : Notes complémentaires (texte libre, optionnel)

**Relations :**
- Relation Many-to-One avec `DossierMedical`
- Relation Many-to-One avec `Medecin`

**Localisation :** `Models/Consultation.cs`

**Point important :** Une consultation appartient à un dossier médical, qui appartient à un patient. La chaîne est : `Patient → DossierMedical → Consultation`

---

## 🔄 FLUX 1 : CRÉER UNE CONSULTATION

```
┌─────────────────────────────────────────────┐
│ Médecin accède à /Consultations/Create      │
│ (depuis un dossier médical ou directement)  │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Consultations/Create
              │ (optionnel : dossierMedicalId)
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Create() [GET]      │
│                                             │
│ 1. Vérifie le rôle (doit être Medecin)     │
│                                             │
│ 2. Récupère le médecin connecté :          │
│    → User → Medecin                         │
│    → MedecinId pré-rempli                  │
│                                             │
│ 3. Charge la liste des dossiers médicaux : │
│    → Tous les DossierMedicals              │
│    → Avec Patient et User                  │
│                                             │
│ 4. Si dossierMedicalId fourni :            │
│    → Pré-sélectionne ce dossier            │
│                                             │
│ 5. Affiche le formulaire                   │
└─────────────┬───────────────────────────────┘
              │
              │ Utilisateur remplit et soumet
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Create() [POST]      │
│                                             │
│ 1. VALIDATION :                            │
│    → DossierMedicalId valide ?             │
│    → MedecinId valide ?                    │
│    → DateConsultation valide ?             │
│                                             │
│ 2. CRÉATION :                              │
│    → Crée la Consultation                  │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/ConsultationsController.cs` - Méthode `Create()` (GET et POST)
- Frontend : `Views/Consultations/Create.cshtml`

**Point important :** Le médecin ne peut créer une consultation que pour un dossier médical existant. Si un patient n'a pas de dossier médical, la secrétaire doit d'abord le créer.

---

## 🔄 FLUX 2 : CONSULTER LES CONSULTATIONS

### Vue Médecin : Ses consultations uniquement

```
┌─────────────────────────────────────────────┐
│ Médecin accède à /Consultations/Index       │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Consultations/Index
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Index()              │
│                                             │
│ 1. Vérifie le rôle (Medecin ou Secretaire) │
│                                             │
│ 2. Si ROLE = "Medecin" :                   │
│    → Récupère le Medecin connecté          │
│    → Filtre par MedecinId                  │
│    → Affiche uniquement SES consultations   │
│                                             │
│ 3. Si ROLE = "Secretaire" :                │
│    → Affiche TOUTES les consultations      │
│                                             │
│ 4. FILTRES DISPONIBLES :                   │
│    → Par nom de patient                    │
│    → Par date                              │
│                                             │
│ 5. Inclut les relations :                  │
│    → DossierMedical → Patient → User       │
│    → Medecin → User                        │
│                                             │
│ 6. Tri par date décroissante               │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Patient (Nom, Prénom)                     │
│ - Date de consultation                      │
│ - Médecin (si Secrétaire)                   │
│ - Diagnostic (tronqué si long)              │
│ - Actions :                                 │
│   → Détails                                 │
│   → Créer ordonnance                        │
│   → Modifier (Médecin uniquement)           │
│   → Supprimer (Médecin uniquement)          │
│   → Imprimer                                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/ConsultationsController.cs` - Méthode `Index()`
- Frontend : `Views/Consultations/Index.cshtml`

---

## 🔄 FLUX 3 : VOIR LES DÉTAILS D'UNE CONSULTATION

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Détails" sur une       │
│ consultation                                │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Consultations/Details/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Details()           │
│                                             │
│ 1. Récupère la Consultation par ID         │
│    → Inclut DossierMedical → Patient       │
│    → Inclut Medecin                         │
│                                             │
│ 2. Affiche toutes les informations :       │
│    → Informations patient                   │
│    → Date et heure                          │
│    → Médecin                                │
│    → Diagnostic complet                     │
│    → Notes complètes                        │
│                                             │
│ 3. Actions disponibles :                   │
│    → Créer ordonnance (Médecin)            │
│    → Modifier (Médecin)                     │
│    → Supprimer (Médecin)                    │
│    → Imprimer                               │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/ConsultationsController.cs` - Méthode `Details()`
- Frontend : `Views/Consultations/Details.cshtml`

---

## 🔄 FLUX 4 : MODIFIER UNE CONSULTATION (MÉDECIN)

```
┌─────────────────────────────────────────────┐
│ Médecin clique "Modifier" sur une          │
│ consultation                                │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Consultations/Edit/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Edit() [GET]        │
│                                             │
│ 1. Vérifie le rôle (doit être Medecin)     │
│                                             │
│ 2. Récupère la Consultation                │
│                                             │
│ 3. Charge les dossiers médicaux            │
│                                             │
│ 4. Affiche le formulaire pré-rempli        │
└─────────────┬───────────────────────────────┘
              │
              │ Médecin modifie et soumet
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Edit() [POST]       │
│                                             │
│ 1. Validation                              │
│                                             │
│ 2. Mise à jour en BDD                      │
│                                             │
│ 3. Succès → Redirection vers Index         │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/ConsultationsController.cs` - Méthode `Edit()` (GET et POST)
- Frontend : `Views/Consultations/Edit.cshtml`

---

## 🔄 FLUX 5 : SUPPRIMER UNE CONSULTATION (MÉDECIN)

### Suppression en cascade

```
┌─────────────────────────────────────────────┐
│ Médecin clique "Supprimer" sur une         │
│ consultation                                │
└─────────────┬───────────────────────────────┘
              │
              │ 1. Confirmation demandée
              │
              ▼
┌─────────────────────────────────────────────┐
│ POST /Consultations/Delete/{id}             │
└─────────────┬───────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.DeleteConfirmed()    │
│                                             │
│ ⚠️ SUPPRESSION EN CASCADE :                │
│                                             │
│ 1. Supprime les Paiements liés :           │
│    → Tous les Paiements de toutes les      │
│       Factures liées à cette Consultation  │
│                                             │
│ 2. Supprime les Factures liées :           │
│    → Toutes les Factures de cette          │
│       Consultation                          │
│                                             │
│ 3. Supprime les OrdonnanceDetails :        │
│    → Tous les détails de toutes les        │
│       Ordonnances liées                     │
│                                             │
│ 4. Supprime les Ordonnances liées :        │
│    → Toutes les Ordonnances de cette       │
│       Consultation                          │
│                                             │
│ 5. Supprime la Consultation elle-même      │
│                                             │
│ 6. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Pourquoi cette cascade ?**
- La base de données utilise `DeleteBehavior.NoAction` (pas de suppression automatique)
- L'application doit gérer manuellement les dépendances
- Ordre important : Enfants d'abord, puis parents

**Localisation :**
- Backend : `Controllers/ConsultationsController.cs` - Méthode `DeleteConfirmed()`
- Frontend : `Views/Consultations/Delete.cshtml`

---

## 🔄 FLUX 6 : IMPRIMER UNE CONSULTATION

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Imprimer" sur une      │
│ consultation                                │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Consultations/Imprimer/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ ConsultationsController.Imprimer()          │
│                                             │
│ 1. Récupère la Consultation                │
│    → Inclut toutes les relations           │
│                                             │
│ 2. Retourne une vue spéciale :             │
│    → Layout = null (pas de menu)           │
│    → Style optimisé pour impression        │
│    → Auto-impression via JavaScript        │
│                                             │
│ 3. Contenu affiché :                       │
│    → En-tête "CONSULTATION MÉDICALE"       │
│    → Informations patient                   │
│    → Date et heure                          │
│    → Médecin                                │
│    → Diagnostic                             │
│    → Notes                                  │
│    → Pied de page (date génération)        │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/ConsultationsController.cs` - Méthode `Imprimer()`
- Frontend : `Views/Consultations/Imprimer.cshtml`

**Fonctionnalités d'impression :**
- Vue sans layout (pas de sidebar, menu, etc.)
- CSS optimisé pour l'impression
- Auto-impression au chargement de la page
- Format professionnel

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`ConsultationsController.cs`**
   - `Index()` : Liste des consultations (avec filtres)
   - `Create()` (GET/POST) : Créer une consultation
   - `Edit()` (GET/POST) : Modifier une consultation
   - `Details()` : Détails d'une consultation
   - `Delete()` (GET) : Affiche la confirmation
   - `DeleteConfirmed()` (POST) : Supprime avec cascade
   - `Imprimer()` : Vue d'impression

### Vues

1. **`Views/Consultations/Index.cshtml`**
   - Liste avec filtres (nom patient, date)
   - Actions selon le rôle

2. **`Views/Consultations/Create.cshtml`**
   - Formulaire de création

3. **`Views/Consultations/Edit.cshtml`**
   - Formulaire de modification

4. **`Views/Consultations/Details.cshtml`**
   - Affichage détaillé avec actions

5. **`Views/Consultations/Delete.cshtml`**
   - Page de confirmation

6. **`Views/Consultations/Imprimer.cshtml`**
   - Vue optimisée pour impression

### Relations avec autres systèmes

1. **Dossier Médical**
   - Une consultation appartient à un dossier médical
   - Créée depuis le dossier médical ou indépendamment

2. **Ordonnances**
   - Une consultation peut avoir plusieurs ordonnances
   - Bouton "Créer ordonnance" depuis une consultation

3. **Factures**
   - Une consultation peut avoir une facture
   - La facture est créée par la secrétaire

---

## 📊 FLUX COMPLET : CRÉER ET UTILISER UNE CONSULTATION

```
1. Médecin accède à /Consultations/Create
   ↓
2. Sélectionne un dossier médical
   (ou depuis /DossierMedicals/Consulter)
   ↓
3. Remplit diagnostic et notes
   ↓
4. Soumet le formulaire
   ↓
5. Consultation créée en BDD
   ↓
6. Consultation visible dans :
   - Index des consultations (Médecin)
   - Détails du dossier médical (Patient)
   ↓
7. Actions possibles :
   → Créer une ordonnance
   → Créer une facture (Secrétaire)
   → Modifier (Médecin)
   → Supprimer (Médecin, avec cascade)
   → Imprimer
```

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Historique complet** : Toutes les consultations d'un patient dans son dossier médical
- **Sécurité** : Seuls les médecins peuvent créer/modifier/supprimer
- **Traçabilité** : Date, heure, médecin enregistrés
- **Impression** : Format professionnel pour archivage
- **Suppression sécurisée** : Gestion manuelle de la cascade

### ⚠️ Limitations actuelles

- Pas de pièces jointes (images, documents)
- Diagnostic et notes en texte libre uniquement
- Pas de template de consultation prédéfini
- Pas de signature électronique

---

## 📝 CONCLUSION

Le système de gestion des consultations est **entièrement fonctionnel** :
- ✅ Création par Médecin uniquement
- ✅ Consultation filtrée selon le rôle
- ✅ Modification et suppression avec sécurité
- ✅ Suppression en cascade pour l'intégrité des données
- ✅ Impression professionnelle
- ✅ Intégration avec dossiers médicaux, ordonnances, factures

**Le système garantit la traçabilité complète des consultations médicales pour chaque patient.**

