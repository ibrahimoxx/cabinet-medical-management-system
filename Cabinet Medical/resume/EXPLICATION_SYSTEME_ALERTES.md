# 🔔 EXPLICATION DU SYSTÈME D'ALERTES

## 📋 Vue d'ensemble

Le système d'alertes permet de notifier les utilisateurs en temps réel de :
1. **Annulations de rendez-vous** (mutuelles entre Patient et Secrétaire)
2. **Rappels automatiques** (24h avant le RDV pour les patients)

---

## 🏗️ ARCHITECTURE DU SYSTÈME

### 1. **Modèle de Données (`Alerte.cs`)**

```csharp
public class Alerte
{
    public int Id { get; set; }
    public string Type { get; set; }           // "AnnulationRDV" ou "RappelRDV"
    public string Message { get; set; }        // Message personnalisé
    public int UserId { get; set; }            // Utilisateur destinataire
    public int? RendezVousId { get; set; }     // RDV concerné (optionnel)
    public bool EstLue { get; set; }           // Statut lu/non lu
    public DateTime DateCreation { get; set; } // Date de création
}
```

**Champs importants :**
- `Type` : Identifie le type d'alerte (AnnulationRDV, RappelRDV)
- `UserId` : L'utilisateur qui recevra l'alerte
- `EstLue` : `false` = Non lue (affiche badge rouge), `true` = Lue

---

## 🔄 FLUX 1 : ALERTES D'ANNULATION

### Scénario 1 : Patient annule un RDV → Alerte pour la Secrétaire

```
┌─────────────┐                    ┌──────────────────┐
│   Patient   │                    │   Secrétaire     │
│             │                    │                  │
│ 1. Clique   │                    │                  │
│ "Annuler"   │                    │                  │
│ sur RDV     │                    │                  │
└─────┬───────┘                    └──────────────────┘
      │
      │ 2. RendezVousController.Annuler()
      │    - Change Statut = "Annule"
      │    - Appelle CreateAlerteAnnulation()
      │
      ▼
┌─────────────────────────────────────┐
│ CreateAlerteAnnulation()            │
│                                     │
│ Si roleQuiAnnule == "Patient" :    │
│   → Récupère toutes les secrétaires│
│   → Crée une alerte pour chacune   │
│   → Message : "Le patient X a      │
│      annulé son RDV du ..."        │
└─────────────┬───────────────────────┘
              │
              │ 3. Alerte sauvegardée en BDD
              │
              ▼
┌─────────────────────────────────────┐
│ Table Alertes                       │
│                                     │
│ Type: "AnnulationRDV"               │
│ Message: "Le patient ... a annulé" │
│ UserId: [ID de la secrétaire]      │
│ EstLue: false                       │
└─────────────┬───────────────────────┘
              │
              │ 4. Badge de notification mis à jour
              │    (via GetUnreadCount toutes les 30s)
              │
              ▼
┌─────────────────────────────────────┐
│ Topbar - Badge rouge "1"            │
│ [🔔] (cliquable vers /Alertes)      │
└─────────────────────────────────────┘
```

**Code source :** `RendezVousController.cs` ligne 476-533

### Scénario 2 : Secrétaire annule un RDV → Alerte pour le Patient

```
┌──────────────────┐                    ┌─────────────┐
│   Secrétaire     │                    │   Patient   │
│                  │                    │             │
│ 1. Clique        │                    │             │
│ "Annuler"        │                    │             │
│ sur RDV          │                    │             │
└────────┬─────────┘                    └─────────────┘
         │
         │ 2. RendezVousController.Annuler()
         │    - Change Statut = "Annule"
         │    - Appelle CreateAlerteAnnulation()
         │
         ▼
┌─────────────────────────────────────┐
│ CreateAlerteAnnulation()            │
│                                     │
│ Si roleQuiAnnule == "Secretaire" : │
│   → Récupère le Patient du RDV      │
│   → Crée une alerte pour le patient │
│   → Message : "Votre RDV du ... a  │
│      été annulé par le secrétariat"│
└─────────────┬───────────────────────┘
              │
              │ 3. Alerte sauvegardée
              │
              ▼
┌─────────────────────────────────────┐
│ Table Alertes                       │
│                                     │
│ Type: "AnnulationRDV"               │
│ Message: "Votre RDV ... a été      │
│           annulé par..."            │
│ UserId: [ID du patient]             │
│ EstLue: false                       │
└─────────────────────────────────────┘
```

---

## ⏰ FLUX 2 : RAPPELS AUTOMATIQUES (24h avant)

### Comment ça fonctionne ?

```
┌─────────────────────────────────────────────┐
│ AU CHARGEMENT DE CHAQUE PAGE                │
│ (_Layout.cshtml)                            │
│                                             │
│ JavaScript appelle automatiquement :        │
│ - checkRappelsRDV() toutes les heures       │
│ - Au chargement initial                     │
└─────────────┬───────────────────────────────┘
              │
              │ Requête AJAX
              │ GET /RendezVous/CheckRappels
              │
              ▼
┌─────────────────────────────────────────────┐
│ RendezVousController.CheckRappels()         │
│                                             │
│ 1. Récupère l'utilisateur connecté         │
│ 2. Si c'est un Patient :                    │
│    a) Récupère tous ses RDV "Planifie"     │
│    b) Filtre ceux dans les 24h suivantes   │
│    c) Pour chaque RDV trouvé :             │
│       - Vérifie si alerte existe déjà      │
│       - Si non, crée une alerte "RappelRDV"│
│       - Message : "Rappel : Vous avez un   │
│         RDV demain le ... avec Dr. ..."    │
└─────────────┬───────────────────────────────┘
              │
              │ 3. Alerte sauvegardée en BDD
              │
              ▼
┌─────────────────────────────────────────────┐
│ Table Alertes                               │
│                                             │
│ Type: "RappelRDV"                           │
│ Message: "Rappel : Vous avez un RDV..."    │
│ UserId: [ID du patient]                     │
│ RendezVousId: [ID du RDV]                   │
│ EstLue: false                               │
└─────────────┬───────────────────────────────┘
              │
              │ 4. Notification navigateur (optionnel)
              │
              ▼
┌─────────────────────────────────────────────┐
│ Si Notification.permission == 'granted' :   │
│   → Affiche une notification système        │
│   → Titre: "Rappel Rendez-vous"            │
│   → Corps: Message de l'alerte             │
└─────────────────────────────────────────────┘
```

**Code source :**
- Backend : `RendezVousController.cs` ligne 559-625
- Frontend : `_Layout.cshtml` ligne 178-211

**Logique de vérification :**
```csharp
// Vérifie les RDV dans les 24h suivantes
var maintenant = DateTime.Now;
var dans24h = maintenant.AddHours(24);

var rdvProches = await _context.RendezVous
    .Where(r => r.PatientId == patient.Id
        && r.Statut == "Planifie"
        && r.DateRdv.Date == dans24h.Date
        && r.DateRdv >= maintenant
        && r.DateRdv <= dans24h)
    .ToListAsync();

// Évite les doublons : vérifie si une alerte existe déjà
var alerteExistante = await _context.Alertes
    .FirstOrDefaultAsync(a => a.UserId == user.Id
        && a.RendezVousId == rdv.Id
        && a.Type == "RappelRDV");
```

---

## 📊 FLUX 3 : AFFICHAGE ET NOTIFICATION EN TEMPS RÉEL

### Badge de notification dans le header

```
┌─────────────────────────────────────────────┐
│ TOPBAR (sur toutes les pages)               │
│                                             │
│ [Titre de la page]     [🔔 3] [Utilisateur]│
│                              ▲              │
│                              │              │
│                              Badge rouge    │
│                              (non lues)     │
└─────────────────────────────────────────────┘
```

**Fonctionnement :**

1. **Au chargement de la page** : JavaScript appelle `loadAlertesCount()`
2. **Toutes les 30 secondes** : Actualisation automatique via `setInterval()`

```javascript
// _Layout.cshtml ligne 156-176
function loadAlertesCount() {
    fetch('/Alertes/GetUnreadCount')  // API GET
        .then(response => response.json())
        .then(data => {
            const badge = document.getElementById('alertesBadge');
            if (data.count > 0) {
                badge.textContent = data.count;  // Affiche "3"
                badge.style.display = 'block';   // Affiche le badge
            } else {
                badge.style.display = 'none';    // Cache le badge
            }
        });
}

// Actualisation toutes les 30 secondes
setInterval(loadAlertesCount, 30000);
```

**Code source :**
- Backend API : `AlertesController.GetUnreadCount()` ligne 60-75
- Frontend : `_Layout.cshtml` ligne 136-139 et 155-176

---

## 📄 FLUX 4 : CONSULTATION DES ALERTES

### Page `/Alertes/Index`

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique sur [🔔 3]               │
└─────────────┬───────────────────────────────┘
              │
              │ Redirection vers /Alertes/Index
              │
              ▼
┌─────────────────────────────────────────────┐
│ AlertesController.Index()                   │
│                                             │
│ 1. Récupère l'utilisateur connecté         │
│ 2. Récupère TOUTES ses alertes             │
│    - Inclut les infos du RDV (Medecin,     │
│      Patient)                              │
│    - Triées par DateCreation DESC          │
│    (plus récentes en premier)              │
└─────────────┬───────────────────────────────┘
              │
              │ Vue Alertes/Index.cshtml
              │
              ▼
┌─────────────────────────────────────────────┐
│ Liste des alertes :                         │
│                                             │
│ ┌─────────────────────────────────────┐    │
│ │ 🔴 Nouveau                          │    │
│ │ ⚠️ AnnulationRDV                    │    │
│ │ Message : "Le patient X a annulé"   │    │
│ │ 📅 16/12/2024 14:30                 │    │
│ │ [Marquer comme lue]                 │    │
│ └─────────────────────────────────────┘    │
│                                             │
│ ┌─────────────────────────────────────┐    │
│ │ ⏰ RappelRDV                         │    │
│ │ Message : "Rappel : Vous avez..."   │    │
│ │ 📅 16/12/2024 10:15                 │    │
│ │ [Marquer comme lue]                 │    │
│ └─────────────────────────────────────┘    │
└─────────────────────────────────────────────┘
```

**Caractéristiques visuelles :**
- **Bordure bleue à gauche** : Alerte non lue (`border-start border-4 border-primary`)
- **Badge "Nouveau"** : Affiché si `EstLue == false`
- **Icônes différentes** :
  - ❌ AnnulationRDV → `bi-x-circle-fill text-danger`
  - ⏰ RappelRDV → `bi-clock-fill text-warning`

**Code source :**
- Backend : `AlertesController.Index()` ligne 20-40
- Frontend : `Alertes/Index.cshtml`

---

## ✅ FLUX 5 : MARQUER UNE ALERTE COMME LUE

```
┌─────────────────────────────────────────────┐
│ Utilisateur clique sur "Marquer comme lue" │
└─────────────┬───────────────────────────────┘
              │
              │ JavaScript : fetch POST
              │ /Alertes/MarquerLue
              │ Body: { id: 5 }
              │
              ▼
┌─────────────────────────────────────────────┐
│ AlertesController.MarquerLue([FromBody] id) │
│                                             │
│ 1. Récupère l'alerte par Id                │
│ 2. Change EstLue = true                    │
│ 3. Sauvegarde en BDD                       │
│ 4. Retourne JSON { success: true }         │
└─────────────┬───────────────────────────────┘
              │
              │ JavaScript reçoit success
              │
              ▼
┌─────────────────────────────────────────────┐
│ location.reload()                           │
│ → Recharge la page                          │
│ → Badge mis à jour (count - 1)              │
│ → Alerte n'affiche plus "Nouveau"           │
└─────────────────────────────────────────────┘
```

**Code source :**
- Backend : `AlertesController.MarquerLue()` ligne 45-55
- Frontend : `Alertes/Index.cshtml` ligne 64-84

---

## 🔧 COMPOSANTS TECHNIQUES

### Contrôleurs

1. **`AlertesController.cs`**
   - `Index()` : Liste des alertes de l'utilisateur
   - `GetUnreadCount()` : API pour le badge (appelée toutes les 30s)
   - `MarquerLue()` : API pour marquer une alerte comme lue

2. **`RendezVousController.cs`**
   - `Annuler()` : Appelle `CreateAlerteAnnulation()` après annulation
   - `CreateAlerteAnnulation()` : Crée les alertes d'annulation
   - `CheckRappels()` : API pour vérifier et créer les rappels 24h

### Vues

1. **`Views/Alertes/Index.cshtml`**
   - Affiche la liste des alertes
   - Bouton "Marquer comme lue" avec JavaScript

2. **`Views/Shared/_Layout.cshtml`**
   - Badge de notification dans le topbar
   - Scripts JavaScript pour actualisation automatique

### Base de données

**Table `Alertes` :**
```sql
CREATE TABLE Alertes (
    Id int PRIMARY KEY IDENTITY(1,1),
    Type nvarchar(max) NOT NULL,        -- "AnnulationRDV" ou "RappelRDV"
    Message nvarchar(max) NOT NULL,
    UserId int NOT NULL,                 -- FK vers Users
    RendezVousId int NULL,               -- FK vers RendezVous (optionnel)
    EstLue bit NOT NULL DEFAULT 0,
    DateCreation datetime2 NOT NULL
);
```

---

## 📊 RÉSUMÉ DES FLUX COMPLETS

### Annulation de RDV (Patient → Secrétaire)

```
1. Patient annule RDV
   ↓
2. RendezVous.Statut = "Annule"
   ↓
3. CreateAlerteAnnulation() crée alerte pour chaque secrétaire
   ↓
4. Alerte sauvegardée (EstLue = false)
   ↓
5. Badge mis à jour dans les 30 secondes
   ↓
6. Secrétaire voit [🔔 1] et clique
   ↓
7. Page /Alertes affiche l'alerte
   ↓
8. Secrétaire clique "Marquer comme lue"
   ↓
9. EstLue = true, badge disparaît
```

### Rappel automatique (24h avant)

```
1. Page chargée (n'importe quelle page)
   ↓
2. JavaScript appelle CheckRappels() au chargement
   ↓
3. Backend vérifie les RDV dans les 24h
   ↓
4. Si RDV trouvé ET alerte n'existe pas déjà
   ↓
5. Crée une alerte "RappelRDV"
   ↓
6. Badge mis à jour
   ↓
7. Notification navigateur (si autorisée)
   ↓
8. Patient voit l'alerte et peut la marquer comme lue
```

---

## ⚙️ CONFIGURATION ET PARAMÈTRES

### Fréquences d'actualisation

- **Badge de notification** : Toutes les **30 secondes** (30000 ms)
- **Vérification des rappels** : Toutes les **heures** (3600000 ms)
- **Au chargement initial** : Les deux vérifications sont exécutées

### Types d'alertes supportés

1. **`AnnulationRDV`** : Créée lors d'une annulation
2. **`RappelRDV`** : Créée automatiquement 24h avant le RDV

**Facilement extensible** : Ajouter de nouveaux types dans le code (ex: "ConsultationAnnulee", "FactureDisponible", etc.)

---

## 🎯 POINTS IMPORTANTS

### ✅ Avantages

- **Temps réel** : Badge mis à jour toutes les 30 secondes
- **Pas de doublons** : Les rappels vérifient l'existence avant création
- **Notifications système** : Support des notifications navigateur
- **Interface intuitive** : Badge visible sur toutes les pages
- **Flexible** : Facilement extensible pour de nouveaux types

### ⚠️ Limitations actuelles

- Les rappels ne sont vérifiés que pour les **patients** (pas pour les médecins)
- Les notifications navigateur nécessitent une autorisation explicite
- Pas de notification par email/SMS (uniquement dans l'application)

---

## 📝 CONCLUSION

Le système d'alertes est **entièrement fonctionnel** et **automatique** :
- ✅ Alertes d'annulation mutuelles (Patient ↔ Secrétaire)
- ✅ Rappels automatiques 24h avant pour les patients
- ✅ Badge de notification en temps réel
- ✅ Interface de consultation et marquage comme lu
- ✅ Prévention des doublons

**Tout fonctionne automatiquement sans intervention manuelle !**

