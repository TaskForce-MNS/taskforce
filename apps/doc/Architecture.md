# 🏗️ Architecture Technique - Projet TaskForce

## 📖 Présentation Générale

TaskForce est une solution multi-plateforme développée dans un environnement **Monorepo**. Le projet est conçu pour l'attribution intelligente de tâches avec une communication en temps réel.

---

## 📂 Structure du Workspace (Root)

Basée sur **pnpm**, l'arborescence racine organise le code par domaine de responsabilité :

* **`.github/`** : Automatisation des builds et CI/CD (GitHub Actions).
* **`.vscode/`** : Automatisation locale (Profils de debug `launch.json` et tâches `tasks.json`).
* **`apps/`** : Contient les différents piliers de l'écosystème :
    * **`Api.Back/`** : Backend principal en .NET 9 (ASP.NET Core, SignalR, EF Core, PostgreSQL, Clean Architecture).
    * **`desktop/`** : Application native propulsée par Tauri (Rust 2021, cargo, faible consommation RAM).
    * **`doc/`** : Documentation technique du projet.
    * **`landing-page/`** : Site vitrine haute performance (Astro, SEO, Lighthouse).
    * **`web-app/`** : Interface utilisateur métier (React + Vite, Zustand, pnpm).
* **`docker-compose.yml`** : Orchestration des services d'infrastructure (PostgreSQL).
* **`taskforce.sln`** : Fichier solution global pour le développement C#.

---

## ⚙️ Architecture du Backend (`Api.Back`)

Le backend suit une structure de **Clean Architecture** simplifiée pour garantir la séparation des préoccupations :

* **`Controllers/`** : Points d'entrée de l'API et gestion des routes REST.
* **`Services/`** : Logique métier pure et algorithmes d'attribution.
* **`DTOs/`** : Objets de transfert de données (Records immuables) pour les échanges avec le Frontend.
* **`Validators/`** : Couche de validation sécurisant les données entrantes.
* **`Hubs/`** : Communication bidirectionnelle en temps réel via SignalR.
* **`Data/`** : Persistance des données et configuration d'Entity Framework Core.
* **`Models/`** : Entités et schéma de la base de données.
* **`Shared/`** : Centralisation des constantes et des routes de l'API pour éviter les "Magic Strings".

---

## 🛠️ Justification des Choix Techniques

| Composant   | Choix         | Justification                                                        |
| :---       | :---          | :---                                                                |
| **Landing** | **Astro**     | Score Lighthouse maximal, poids minimal et SEO optimal.              |
| **Frontend**| **React**     | Contrôle total, absence de dépendances superflues et agilité.        |
| **Backend** | **C# .NET**   | Performance brute, typage fort et gestion native du temps réel (SignalR). |
| **Database**| **PostgreSQL**| Rigueur du modèle relationnel pour l'intégrité des données.          |
| **Desktop** | **Tauri**     | Sécurité Rust et consommation RAM extrêmement faible.                |

---

## 🚀 Outils de Soutien & Qualité

* **Gestion d'état** : TanStack Zustand (React).
* **Analyse de code** : SonarQube for IDE (SonarLint) et .NET Analyzers pour maintenir un code "propre".
* **Infrastructure** : Isolation des services via Docker pour un environnement de dev sain.
* **DevOps** : Compilation automatisée des `.exe` Tauri dans le Cloud via GitHub Actions.

---

## 🌐 Communication et Interconnexion

* **SignalR** : Communication temps réel entre le backend et les clients (web-app, desktop).
* **DTOs** : Format d’échange standardisé entre les modules.
* **API REST** : Exposition des endpoints pour la web-app, le desktop et l’intégration externe.

---

## 🏗️ Déploiement et Scalabilité

* **Docker Compose** : Orchestration locale des services (API, base de données).
* **CI/CD GitHub Actions** : Build, tests, déploiement automatisés pour chaque module.
* **Astro/React/Tauri** : Build optimisé pour chaque cible (web, desktop, landing).

---

## 📦 Modules et Technologies

* **Api.Back** : ASP.NET Core 9, SignalR, EF Core, PostgreSQL
* **web-app** : React, Zustand, Vite, pnpm
* **landing-page** : Astro
* **desktop** : Tauri, Rust 2021
* **taskforce** : Scripts JS, npm

---

## 📚 Documentation

Chaque module doit être documenté dans le dossier `doc/` pour faciliter la prise en main et la contribution.