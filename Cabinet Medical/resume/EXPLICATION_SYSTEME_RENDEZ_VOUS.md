# 📅 EXPLICATION DU SYSTÈME DE GESTION DES RENDEZ-VOUS

## 📋 Vue d'ensemble

Le système de gestion des rendez-vous permet de :
1. **Planifier des rendez-vous** médicaux (Secrétaire et Patient)
2. **Consulter les rendez-vous** (tous les rôles selon leurs permissions)
3. **Modifier les rendez-vous** (Secrétaire uniquement)
4. **Annuler les rendez-vous** (Secrétaire et Patient)
5. **Calculer la disponibilité** du médecin en temps réel
6. **Valider les créneaux** (jours ouvrables, horaires, pas de doublons)

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données (`RendezVous.cs`)**

**Champs importants :**
- `PatientId` : ID du patient (obligatoire)
- `MedecinId` : ID du médecin (obligatoire)
- `DateRdv` : Date du rendez-vous (obligatoire)
- `HeureRdv` : Heure du rendez-vous (format TimeSpan, obligatoire)
- `Statut` : Statut du RDV ("Planifie", "Annule", "Termine")
- `Motif` : Raison/motif du rendez-vous (optionnel)

**Relations :**
- Relation Many-to-One avec `Patient`
- Relation Many-to-One avec `Medecin`

**Localisation :** `Models/RendezVous.cs`

---

## 🔄 FLUX 1 : CRÉER UN RENDEZ-VOUS

### Scénario 1 : Secrétaire crée un RDV

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /RendezVous/Create      │
└─────────────┬───────────────────────────────┘
              │
              │ 1. Page affiche :
              │    - Liste déroulante Patients
              │    - Liste déroulante Médecins
              │    - Sélecteur de date
              │    - Sélecteur d'heure (dynamique)
              │    - Champ motif
              │
              ▼
┌─────────────────────────────────────────────┐
│ Utilisateur sélectionne :                   │
│ 1. Patient (depuis la liste)                │
│ 2. Médecin                                  │
│ 3. Date                                     │
│                                             │
│ → JavaScript appelle GetAvailableSlots()    │
│ → Remplit automatiquement les heures        │
└─────────────┬───────────────────────────────┘
              │
              │ 2. Utilisateur remplit le formulaire
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.Create() [POST]        │
│                                             │
│ 1. VALIDATION :                            │
│    → Date dans le futur ?                  │
│    → Jour ouvrable (Lundi-Vendredi) ?      │
│    → Heure entre 08:00 et 17:00 ?          │
│    → Créneau de 30 minutes ?               │
│    → Pas de doublon (même médecin/heure) ? │
│                                             │
│ 2. CRÉATION :                              │
│    → Crée le RendezVous                    │
│    → Statut = "Planifie"                   │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers /RendezVous/Index    │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/RendezVousController.cs` - Méthode `Create()` (GET et POST)
- Frontend : `Views/RendezVous/Create.cshtml`

### Scénario 2 : Patient crée un RDV

**Différences avec le scénario Secrétaire :**
- Le patient est **automatiquement pré-sélectionné** (ne peut pas choisir d'autre patient)
- Le patient choisit le médecin et la date
- Les créneaux disponibles s'affichent automatiquement
- Le reste du processus est identique

---

## 🔄 FLUX 2 : CONSULTER LES RENDEZ-VOUS

### Vue Secrétaire : Tous les rendez-vous

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /RendezVous/Index       │
└─────────────┬───────────────────────────────┘
              │
              │ GET /RendezVous/Index
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.Index()                │
│                                             │
│ 1. Vérifie le rôle (doit être Secretaire)  │
│                                             │
│ 2. Récupère TOUS les rendez-vous :         │
│    → Inclut Patient (avec User)            │
│    → Inclut Medecin (avec User)            │
│    → Triés par date décroissante           │
│                                             │
│ 3. FILTRES DISPONIBLES :                   │
│    → Par nom de patient                    │
│    → Par nom de médecin                    │
│    → Par statut (Planifie/Annule/Termine)  │
│    → Par date                              │
│                                             │
│ 4. Retourne la liste filtrée               │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Patient (Nom, Prénom)                     │
│ - Médecin (Nom, Prénom, Spécialité)        │
│ - Date et Heure                             │
│ - Statut (badge coloré)                     │
│ - Actions (Modifier, Annuler, Détails)     │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/RendezVousController.cs` - Méthode `Index()`
- Frontend : `Views/RendezVous/Index.cshtml`

### Vue Médecin/Patient : Mes rendez-vous

```
┌─────────────────────────────────────────────┐
│ Utilisateur accède à /RendezVous/MesRendezVous│
└─────────────┬───────────────────────────────┘
              │
              │ GET /RendezVous/MesRendezVous
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.MesRendezVous()        │
│                                             │
│ 1. Récupère l'utilisateur connecté         │
│                                             │
│ 2. Si ROLE = "Medecin" :                   │
│    → Récupère le Medecin lié au User       │
│    → Filtre RDV par MedecinId              │
│    → Filtres : nom patient, statut, date   │
│                                             │
│ 3. Si ROLE = "Patient" :                   │
│    → Récupère le Patient lié au User       │
│    → Filtre RDV par PatientId              │
│    → Filtres : nom médecin, statut, date   │
│                                             │
│ 4. Retourne la liste filtrée               │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/RendezVousController.cs` - Méthode `MesRendezVous()`
- Frontend : `Views/RendezVous/MesRendezVous.cshtml`

---

## 🔄 FLUX 3 : CALCUL DE DISPONIBILITÉ EN TEMPS RÉEL

### Comment fonctionne l'API GetAvailableSlots ?

```
┌─────────────────────────────────────────────┐
│ Utilisateur sélectionne Date + Médecin      │
│ dans le formulaire de création              │
└─────────────┬───────────────────────────────┘
              │
              │ JavaScript détecte changement
              │
              ▼
┌─────────────────────────────────────────────┐
│ fetch('/RendezVous/GetAvailableSlots?      │
│        medecinId=X&date=2024-12-20')       │
└─────────────┬───────────────────────────────┘
              │
              │ GET /RendezVous/GetAvailableSlots
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.GetAvailableSlots()    │
│                                             │
│ 1. VÉRIFICATIONS :                         │
│    → Jour ouvrable (Lundi-Vendredi) ?      │
│    → Si week-end → Retourne liste vide     │
│                                             │
│ 2. GÉNÉRATION DES CRÉNEAUX :               │
│    → Créneaux de 08:00 à 16:30             │
│    → Par pas de 30 minutes                 │
│    → Total : 17 créneaux/jour              │
│                                             │
│ 3. RÉCUPÉRATION DES CRÉNEAUX PRIS :        │
│    → Requête BDD : RendezVous              │
│    → Filtre : même médecin, même date      │
│    → Exclut les RDV "Annule"               │
│                                             │
│ 4. FILTRAGE :                              │
│    → Supprime les créneaux déjà pris       │
│    → Si date = aujourd'hui :               │
│       Supprime les heures passées          │
│       + 30 minutes de buffer               │
│                                             │
│ 5. CALCUL DU TAUX D'OCCUPATION :           │
│    → (Créneaux pris / Total) * 100         │
│                                             │
│ 6. RETOUR JSON :                           │
│    {                                        │
│      availableSlots: ["08:00", "08:30"...],│
│      medecinDisponible: true,              │
│      tauxOccupation: 35.3                  │
│    }                                        │
└─────────────┬───────────────────────────────┘
              │
              │ JavaScript reçoit la réponse
              │
              ▼
┌─────────────────────────────────────────────┐
│ MISE À JOUR DE L'INTERFACE :                │
│                                             │
│ 1. Remplit le <select> des heures          │
│    avec les créneaux disponibles           │
│                                             │
│ 2. Affiche le statut de disponibilité :    │
│    → "Médecin disponible - X créneaux libres"│
│    → "Médecin non disponible"              │
│    → Taux d'occupation                     │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend API : `Controllers/RendezVousController.cs` - Méthode `GetAvailableSlots()`
- Frontend : `Views/RendezVous/Create.cshtml` (JavaScript)

**Règles de disponibilité :**
- Jours : Lundi à Vendredi uniquement
- Heures : 08:00 à 17:00 (dernier créneau à 16:30)
- Créneaux : 30 minutes (08:00, 08:30, 09:00, etc.)
- Buffer : 30 minutes minimum avant le prochain créneau (si date = aujourd'hui)

---

## 🔄 FLUX 4 : MODIFIER UN RENDEZ-VOUS (SECRÉTAIRE)

```
┌─────────────────────────────────────────────┐
│ Secrétaire clique "Modifier" sur un RDV     │
└─────────────┬───────────────────────────────┘
              │
              │ GET /RendezVous/Edit/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.Edit() [GET]           │
│                                             │
│ 1. Vérifie le rôle (doit être Secretaire)  │
│                                             │
│ 2. Récupère le RendezVous par ID           │
│                                             │
│ 3. Charge les listes :                      │
│    → Patients (actifs)                      │
│    → Medecins (actifs)                      │
│                                             │
│ 4. Affiche le formulaire pré-rempli        │
└─────────────┬───────────────────────────────┘
              │
              │ Utilisateur modifie et soumet
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.Edit() [POST]          │
│                                             │
│ 1. Validation (même règles que Create)     │
│                                             │
│ 2. Mise à jour en BDD                      │
│                                             │
│ 3. Succès → Redirection vers Index         │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/RendezVousController.cs` - Méthode `Edit()` (GET et POST)
- Frontend : `Views/RendezVous/Edit.cshtml`

---

## 🔄 FLUX 5 : ANNULER UN RENDEZ-VOUS

### Scénario 1 : Patient annule

```
┌─────────────────────────────────────────────┐
│ Patient accède à MesRendezVous              │
│ Clique "Annuler" sur un RDV "Planifie"      │
└─────────────┬───────────────────────────────┘
              │
              │ POST /RendezVous/Annuler/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.Annuler()              │
│                                             │
│ 1. Vérifie le rôle (Patient ou Secretaire) │
│                                             │
│ 2. Récupère le RendezVous                  │
│                                             │
│ 3. MODIFICATION :                          │
│    → Change Statut = "Annule"              │
│    → Sauvegarde en BDD                     │
│                                             │
│ 4. CRÉATION ALERTE :                       │
│    → Appelle CreateAlerteAnnulation()      │
│    → Type: "AnnulationRDV"                 │
│    → Destinataire: Toutes les secrétaires  │
│    → Message: "Le patient X a annulé..."   │
│                                             │
│ 5. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers MesRendezVous        │
└─────────────────────────────────────────────┘
```

### Scénario 2 : Secrétaire annule

**Différences :**
- L'alerte est créée pour le **Patient** et le **Médecin**
- Message : "Votre RDV du ... a été annulé par le secrétariat"

**Localisation :**
- Backend : `Controllers/RendezVousController.cs` - Méthode `Annuler()`
- Frontend : Bouton dans `Views/RendezVous/Index.cshtml` et `MesRendezVous.cshtml`

---

## 🔒 RÈGLES DE VALIDATION

### Validation des rendez-vous

Le système vérifie automatiquement :

1. **Jour ouvrable**
   - ❌ Samedi et Dimanche refusés
   - ✅ Lundi à Vendredi acceptés

2. **Date**
   - ❌ Dates passées refusées
   - ✅ Dates futures ou aujourd'hui acceptées

3. **Heures**
   - ❌ Avant 08:00 ou après 17:00 refusées
   - ✅ Entre 08:00 et 16:30 acceptées

4. **Créneaux**
   - ❌ Créneaux non multiples de 30 minutes refusés
   - ✅ 08:00, 08:30, 09:00, etc. acceptés

5. **Doublons**
   - ❌ Même médecin + même date + même heure refusés
   - ✅ Créneaux libres acceptés

**Localisation :**
- Backend : `Controllers/RendezVousController.cs` - Méthode `ValidateAppointment()`

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`RendezVousController.cs`**
   - `Index()` : Liste tous les RDV (Secrétaire)
   - `MesRendezVous()` : RDV du médecin ou du patient
   - `Create()` (GET/POST) : Créer un RDV
   - `Edit()` (GET/POST) : Modifier un RDV (Secrétaire)
   - `Details()` : Détails d'un RDV
   - `Annuler()` : Annuler un RDV
   - `GetAvailableSlots()` : API pour les créneaux disponibles
   - `ValidateAppointment()` : Validation des règles métier

### Vues

1. **`Views/RendezVous/Index.cshtml`**
   - Liste complète pour Secrétaire avec filtres

2. **`Views/RendezVous/MesRendezVous.cshtml`**
   - Liste filtrée pour Médecin ou Patient

3. **`Views/RendezVous/Create.cshtml`**
   - Formulaire avec sélection dynamique des heures

4. **`Views/RendezVous/Edit.cshtml`**
   - Formulaire de modification (Secrétaire)

5. **`Views/RendezVous/Details.cshtml`**
   - Affichage détaillé d'un RDV

### Base de données

**Table `RendezVous` :**
- Clé primaire : `Id`
- Clés étrangères : `PatientId`, `MedecinId`
- Index recommandés : `DateRdv`, `MedecinId`, `PatientId` pour les performances

---

## 📊 FLUX COMPLET : CRÉATION D'UN RDV PAR PATIENT

```
1. Patient accède à /RendezVous/Create
   ↓
2. Patient sélectionné automatiquement
   ↓
3. Patient choisit un médecin
   ↓
4. Patient choisit une date
   ↓
5. JavaScript appelle GetAvailableSlots()
   ↓
6. Backend calcule les créneaux disponibles
   ↓
7. JavaScript remplit la liste des heures
   ↓
8. Patient choisit une heure disponible
   ↓
9. Patient remplit le motif (optionnel)
   ↓
10. Soumission du formulaire
    ↓
11. Validation côté serveur
    ↓
12. Vérification pas de doublon
    ↓
13. Création du RendezVous
    ↓
14. Sauvegarde en BDD (Statut = "Planifie")
    ↓
15. Message de succès
    ↓
16. Redirection vers MesRendezVous
```

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Interface intuitive** : Sélection dynamique des créneaux
- **Prévention des doublons** : Impossible de réserver un créneau pris
- **Validation complète** : Côté client et serveur
- **Disponibilité en temps réel** : Calcul automatique
- **Filtres avancés** : Recherche facile des RDV

### ⚠️ Limitations actuelles

- Disponibilité calculée uniquement pour Lundi-Vendredi 08:00-17:00
- Pas de gestion des jours fériés
- Pas de confirmation par email/SMS
- Les créneaux sont fixes (pas de personnalisation par médecin)

---

## 📝 CONCLUSION

Le système de gestion des rendez-vous est **entièrement fonctionnel** :
- ✅ Création par Secrétaire et Patient
- ✅ Consultation filtrée selon le rôle
- ✅ Modification par Secrétaire
- ✅ Annulation avec alertes automatiques
- ✅ Calcul de disponibilité en temps réel
- ✅ Validation complète des règles métier

**Le système garantit qu'aucun doublon ne peut être créé et que seuls les créneaux valides sont proposés.**

