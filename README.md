# 🏥 Cabinet Medical Management System (CMMS)

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) 
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![MicrosoftSQLServer](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
![Bootstrap](https://img.shields.io/badge/bootstrap-%238511FA.svg?style=for-the-badge&logo=bootstrap&logoColor=white)

> **Plateforme web complète de gestion de cabinet médical développée avec ASP.NET Core MVC 10.0.**
> 
> *Comprehensive medical clinic management system built with ASP.NET Core MVC 10.0.*

---

## 📖 About The Project

Ce projet digitalise l'intégralité du cycle de soin médical, de la prise de rendez-vous initiale à la facturation, en passant par la gestion des dossiers médicaux et des ordonnances. L'application est sécurisée par un système de contrôle d'accès basé sur les rôles (RBAC) à 4 niveaux.

### 🌟 Key Features

*   🔐 **RBAC (Role-Based Access Control) :** 4 rôles distincts (Administrateur, Médecin, Secrétaire, Patient) avec permissions granulaires.
*   📅 **Gestion Intelligente des Rendez-vous :** Réservation en ligne, vérification dynamique des disponibilités (créneaux de 30 min) et prévention des conflits.
*   🔔 **Système de Notifications en Temps Réel :** Moteur d'alertes internes pour les annulations de RDV et rappels automatiques 24h avant les consultations.
*   📂 **Dossiers Médicaux Électroniques (DME/EHR) :** Historique complet centralisé des antécédents, diagnostics et notes médicales.
*   💊 **Prescriptions & Ordonnances :** Génération d'ordonnances dématérialisées (médicaments, analyses, radiologie) intégrant un module d'impression.
*   💳 **Facturation Automatisée :** Module de suivi financier lié aux consultations avec gestion des paiements (en ligne/espèces) et calcul des restes à payer.

---

## 🛠️ Built With

L'application repose sur une architecture MVC propre et moderne :

*   **Framework Backend :** ASP.NET Core MVC 10.0
*   **ORM :** Entity Framework Core (Code-First Migrations)
*   **Base de Données :** Microsoft SQL Server
*   **Frontend :** Razor Views, Bootstrap 5.3.3, jQuery, Animate.css, Vanilla CSS3

### Architecture Highlights
*   **Filtrage Avancé :** Forte utilisation d'`IQueryable` pour des recherches multi-critères performantes côté serveur.
*   **Intégrité des Données :** Logique de suppression en cascade entièrement gérée au niveau des contrôleurs pour protéger les archives médicales (`DeleteBehavior.NoAction`).

---

## 🚀 Getting Started

### Prerequisites

*   .NET SDK 10.0
*   SQL Server (LocalDB ou une instance complète)
*   Visual Studio 2022 (recommandé) ou Visual Studio Code

### Installation & Run

1.  Clone the repository
    ```sh
    git clone https://github.com/ibrahimoxx/cabinet-medical-management-system.git
    ```
2.  Open the solution (`Cabinet Medical.sln`) in Visual Studio.
3.  Update the database connection string in `appsettings.json` if necessary (by default it uses `Trusted_Connection=True` on your local machine).
4.  Apply Entity Framework Core Migrations to create the database:
    *   Via Package Manager Console:
        ```ps
        Update-Database
        ```
    *   Or via .NET CLI:
        ```sh
        dotnet ef database update
        ```
5.  Run the application (`F5` in Visual Studio).

---

## 👥 User Roles & Capabilities

| Role | Responsibilities |
| :--- | :--- |
| **👑 Admin** | System configuration, User management (CRUD for all roles), System monitoring dashboards. |
| **👨‍⚕️ Médecin (Doctor)** | Manage consultations, write prescriptions (Ordonnances), view patient medical history, access personal schedule. |
| **👩‍💼 Secrétaire (Secretary)** | Patient registration, schedule management, appointment cancellation, billing creation. |
| **👤 Patient** | Online appointment booking, view personal medical records (read-only), download prescriptions and invoices. |

---

## 📄 License & Credits

Projet développé dans le cadre d'un cycle universitaire / Portfolio. (2024-2025).

*   **Lead Full-Stack Developer:** Ibrahimoxx
*   **Version:** 1.3
