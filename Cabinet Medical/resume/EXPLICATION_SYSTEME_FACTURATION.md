# 💰 EXPLICATION DU SYSTÈME DE FACTURATION ET PAIEMENTS

## 📋 Vue d'ensemble

Le système de facturation et paiements permet de :
1. **Créer des factures** liées à une consultation (Secrétaire uniquement)
2. **Consulter les factures** (Secrétaire voit toutes, Patient voit les siennes)
3. **Enregistrer des paiements** (Patient - en ligne ou espèces)
4. **Suivre le statut** des factures (Payée / Non Payée)
5. **Imprimer des factures** (tous les rôles autorisés)

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données**

#### `Facture.cs`
**Champs importants :**
- `PatientId` : ID du patient (obligatoire)
- `ConsultationId` : ID de la consultation associée (obligatoire)
- `Montant` : Montant total de la facture (decimal, précision 10,2 - obligatoire)
- `DateFacture` : Date de création (par défaut = maintenant)
- `Statut` : Statut de la facture ("Payee" ou "NonPayee" - obligatoire)

**Relations :**
- Relation Many-to-One avec `Patient`
- Relation Many-to-One avec `Consultation`
- Relation One-to-Many avec `Paiement` (une facture peut avoir plusieurs paiements)

#### `Paiement.cs`
**Champs importants :**
- `FactureId` : ID de la facture parente (obligatoire)
- `ModePaiement` : Mode de paiement ("EnLigne" ou "Espece" - obligatoire)
- `Montant` : Montant du paiement (decimal, précision 10,2 - obligatoire)
- `DatePaiement` : Date du paiement (par défaut = maintenant)

**Localisation :** `Models/Facture.cs` et `Models/Paiement.cs`

**Point important :** Une facture peut être payée en plusieurs fois (plusieurs paiements). Le statut "Payee" est calculé automatiquement si la somme des paiements >= montant de la facture.

---

## 🔄 FLUX 1 : CRÉER UNE FACTURE (SECRÉTAIRE)

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /Factures/Create        │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Factures/Create
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.Create() [GET]           │
│                                             │
│ 1. Vérifie le rôle (doit être Secretaire)  │
│                                             │
│ 2. Charge les listes :                      │
│    → Patients (actifs)                      │
│    → Consultations (avec Patient et        │
│       Médecin)                              │
│                                             │
│ 3. Affiche le formulaire                   │
└─────────────┬───────────────────────────────┘
              │
              │ Utilisateur remplit :
              │ - Patient
              │ - Consultation (filtre selon patient)
              │ - Montant
              │ - Statut (NonPayee par défaut)
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.CreatePost() [POST]      │
│                                             │
│ 1. VALIDATION :                            │
│    → PatientId valide ?                    │
│    → ConsultationId valide ?               │
│    → Montant > 0 ?                         │
│    → Consultation appartient au patient    │
│       sélectionné ?                         │
│                                             │
│ 2. CRÉATION :                              │
│    → Crée la Facture                       │
│    → Statut = "NonPayee" (par défaut)     │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers Index                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/FacturesController.cs` - Méthodes `Create()` (GET) et `CreatePost()` (POST)
- Frontend : `Views/Factures/Create.cshtml`

**Validation spéciale :** Le système vérifie que la consultation sélectionnée appartient bien au patient choisi, pour éviter les erreurs.

---

## 🔄 FLUX 2 : CONSULTER LES FACTURES

### Vue Secrétaire : Toutes les factures

```
┌─────────────────────────────────────────────┐
│ Secrétaire accède à /Factures/Index         │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Factures/Index
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.Index()                  │
│                                             │
│ 1. Vérifie le rôle (doit être Secretaire)  │
│                                             │
│ 2. Récupère TOUTES les factures :          │
│    → Inclut Patient → User                 │
│    → Inclut Consultation → Medecin → User  │
│    → Tri par date décroissante             │
│                                             │
│ 3. Affiche toutes les factures             │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Patient                                   │
│ - Consultation (Date, Médecin)              │
│ - Montant                                   │
│ - Statut (badge coloré)                     │
│ - Date                                      │
│ - Actions :                                 │
│   → Détails                                 │
│   → Imprimer                                │
└─────────────────────────────────────────────┘
```

### Vue Patient : Mes factures

```
┌─────────────────────────────────────────────┐
│ Patient accède à /Factures/MesFactures      │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Factures/MesFactures
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.MesFactures()            │
│                                             │
│ 1. Vérifie le rôle (doit être Patient)     │
│                                             │
│ 2. Récupère le Patient connecté            │
│                                             │
│ 3. Filtre les factures :                   │
│    → Par PatientId                         │
│    → Inclut Consultation → Medecin         │
│    → Inclut TOUS les Paiements             │
│                                             │
│ 4. Calcul pour chaque facture :            │
│    → Montant total payé                    │
│    → Reste à payer                         │
│    → Statut calculé (Payee si reste = 0)   │
│                                             │
│ 5. Tri par date décroissante               │
└─────────────┬───────────────────────────────┘
              │
              │ Vue MesFactures.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Tableau affichant :                         │
│ - Date de consultation                      │
│ - Médecin                                   │
│ - Montant total                             │
│ - Montant payé                              │
│ - Reste à payer                             │
│ - Statut (Payée / Non Payée)               │
│ - Actions :                                 │
│   → Payer (si Non Payée)                   │
│   → Détails                                 │
│   → Imprimer                                │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/FacturesController.cs` - Méthodes `Index()` et `MesFactures()`
- Frontend : `Views/Factures/Index.cshtml` et `Views/Factures/MesFactures.cshtml`

---

## 🔄 FLUX 3 : PAYER UNE FACTURE (PATIENT)

```
┌─────────────────────────────────────────────┐
│ Patient consulte MesFactures                │
│ Clique "Payer" sur une facture Non Payée   │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Factures/Payer/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.Payer() [GET]            │
│                                             │
│ 1. Vérifie le rôle (doit être Patient)     │
│                                             │
│ 2. Récupère la Facture                     │
│    → Inclut Patient (vérifie que c'est    │
│       le bon patient)                       │
│    → Inclut TOUS les Paiements existants  │
│                                             │
│ 3. Calcule :                               │
│    → Montant total de la facture           │
│    → Montant déjà payé                     │
│    → Reste à payer                         │
│                                             │
│ 4. Affiche le formulaire de paiement       │
└─────────────┬───────────────────────────────┘
              │
              │ Patient remplit :
              │ - Mode de paiement (EnLigne/Espece)
              │ - Montant (≤ reste à payer)
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.Payer() [POST]           │
│                                             │
│ 1. VALIDATION :                            │
│    → Montant > 0 ?                         │
│    → Montant ≤ reste à payer ?             │
│    → ModePaiement valide ?                 │
│                                             │
│ 2. CRÉATION DU PAIEMENT :                  │
│    → Crée un Paiement                      │
│    → Lie à la Facture                      │
│    → Sauvegarde en BDD                     │
│                                             │
│ 3. VÉRIFICATION STATUT :                   │
│    → Calcule nouveau montant payé          │
│    → Si montant payé >= montant facture :  │
│       Change Statut = "Payee"              │
│       Mise à jour Facture                  │
│                                             │
│ 4. SUCCÈS :                                │
│    → Message de confirmation               │
│    → Redirection vers MesFactures          │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/FacturesController.cs` - Méthode `Payer()` (GET et POST)
- Frontend : `Views/Factures/Payer.cshtml`

**Fonctionnalité importante :** Le système permet de payer une facture en plusieurs fois. Le statut "Payee" est automatiquement mis à jour quand le total des paiements atteint le montant de la facture.

---

## 🔄 FLUX 4 : VOIR LES DÉTAILS D'UNE FACTURE

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Détails" sur une      │
│ facture                                     │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Factures/Details/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.Details()                │
│                                             │
│ 1. Récupère la Facture par ID              │
│    → Inclut Patient → User                 │
│    → Inclut Consultation → Medecin         │
│    → Inclut TOUS les Paiements             │
│                                             │
│ 2. Calcule :                               │
│    → Montant total                         │
│    → Montant payé (somme des paiements)    │
│    → Reste à payer                         │
│    → Statut (Payee ou NonPayee)            │
│                                             │
│ 3. Affiche toutes les informations :       │
│    → Informations patient                   │
│    → Informations consultation              │
│    → Montant et statut                      │
│    → Liste des paiements :                 │
│       - Date                                │
│       - Mode de paiement                    │
│       - Montant                             │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/FacturesController.cs` - Méthode `Details()`
- Frontend : `Views/Factures/Details.cshtml`

---

## 🔄 FLUX 5 : IMPRIMER UNE FACTURE

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique "Imprimer" sur une      │
│ facture                                     │
└─────────────┬───────────────────────────────┘
              │
              │ GET /Factures/Imprimer/{id}
              │
              ▼
┌─────────────────────────────────────────────┐
│ FacturesController.Imprimer()               │
│                                             │
│ 1. Récupère la Facture                     │
│    → Inclut toutes les relations           │
│    → Inclut TOUS les Paiements             │
│                                             │
│ 2. Calcule les totaux                      │
│                                             │
│ 3. Retourne une vue spéciale :             │
│    → Layout = null (pas de menu)           │
│    → Style optimisé pour impression        │
│    → Auto-impression via JavaScript        │
│                                             │
│ 4. Contenu affiché :                       │
│    → En-tête "FACTURE"                     │
│    → Informations patient                   │
│    → Informations consultation              │
│    → Montant total                          │
│    → Détail des paiements                   │
│    → Reste à payer                          │
│    → Pied de page (date génération)        │
└─────────────────────────────────────────────┘
```

**Localisation :**
- Backend : `Controllers/FacturesController.cs` - Méthode `Imprimer()`
- Frontend : `Views/Factures/Imprimer.cshtml`

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`FacturesController.cs`**
   - `Index()` : Liste toutes les factures (Secrétaire)
   - `MesFactures()` : Liste des factures du patient
   - `Create()` (GET) : Formulaire de création
   - `CreatePost()` (POST) : Création avec validation
   - `Details()` : Détails d'une facture
   - `Payer()` (GET/POST) : Enregistrer un paiement
   - `Imprimer()` : Vue d'impression

### Vues

1. **`Views/Factures/Index.cshtml`**
   - Liste complète pour Secrétaire

2. **`Views/Factures/MesFactures.cshtml`**
   - Liste pour Patient avec calculs

3. **`Views/Factures/Create.cshtml`**
   - Formulaire de création

4. **`Views/Factures/Payer.cshtml`**
   - Formulaire de paiement

5. **`Views/Factures/Details.cshtml`**
   - Affichage détaillé

6. **`Views/Factures/Imprimer.cshtml`**
   - Vue optimisée pour impression

### Base de données

**Tables :**
- `Factures` : Factures principales
- `Paiements` : Paiements liés aux factures

**Contraintes :**
- `Montant` et `Paiement.Montant` : Decimal(10,2) pour précision
- Clés étrangères : `PatientId`, `ConsultationId`, `FactureId`

---

## 📊 FLUX COMPLET : FACTURATION ET PAIEMENT

```
1. Consultation effectuée par un Médecin
   ↓
2. Secrétaire crée une facture :
   → Sélectionne Patient
   → Sélectionne Consultation
   → Saisit le montant
   → Statut = "NonPayee"
   ↓
3. Facture créée en BDD
   ↓
4. Patient voit la facture dans "Mes factures"
   ↓
5. Patient clique "Payer"
   ↓
6. Choix du mode de paiement (EnLigne/Espece)
   ↓
7. Saisie du montant (peut être partiel)
   ↓
8. Paiement enregistré
   ↓
9. Système vérifie :
   → Si total payé >= montant facture
   → Statut changé en "Payee"
   ↓
10. Facture marquée comme payée
    (ou reste partiellement payée)
```

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Paiements multiples** : Possibilité de payer en plusieurs fois
- **Suivi automatique** : Statut mis à jour automatiquement
- **Traçabilité** : Historique complet des paiements
- **Sécurité** : Validation que consultation appartient au patient
- **Impression** : Format professionnel pour archivage

### ⚠️ Limitations actuelles

- Pas d'intégration avec une passerelle de paiement réelle (paiement en ligne simulé)
- Pas de reçu de paiement généré
- Pas de remboursement géré
- Pas de gestion des acomptes/avances

---

## 📝 CONCLUSION

Le système de facturation et paiements est **entièrement fonctionnel** :
- ✅ Création de factures par Secrétaire
- ✅ Consultation différenciée selon le rôle
- ✅ Paiements multiples par Patient
- ✅ Mise à jour automatique du statut
- ✅ Impression professionnelle
- ✅ Intégration avec consultations

**Le système garantit un suivi complet des factures et paiements pour chaque consultation médicale.**

