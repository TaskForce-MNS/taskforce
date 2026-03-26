# 🧭 Guide de Prise en Main du Code TaskForce

Bienvenue dans le projet TaskForce ! Ce guide est destiné à tout nouveau développeur souhaitant comprendre rapidement l’architecture, les modules et les points clés du code.

---

## 🏗️ Prérequis Rapides

Avant de commencer, assure-toi que les services suivants sont opérationnels :
1. **Docker** : Doit être lancé pour la base de données PostgreSQL.
2. **pnpm** : Installé globalement pour la gestion du monorepo.
3. **.NET SDK** : Installé pour l'API C#.

---

## 🛠️ Automatisation VS Code (.vscode)

Nous avons configuré VS Code pour transformer l'éditeur en un véritable centre de contrôle. Tu n'as quasiment plus besoin de taper de commandes manuellement.

### 1. Gestion de la Base de Données (Tasks)
Pour gérer les migrations Entity Framework Core sans erreur de frappe, utilise le menu **Tâches** (`Ctrl+Shift+P` > `Tasks: Run Task`) :

| Nom de la Tâche | Action |
| :--- | :--- |
| **EF: Add Migration** | Crée une nouvelle "recette" SQL basée sur tes changements dans `Models/`. |
| **EF: Update Database** | Applique les migrations en attente sur ta base PostgreSQL. |

### 2. Profils de Lancement (Launch)
Dans l'onglet **"Exécuter et déboguer"** (icône insecte), tu trouveras deux boutons principaux :

* **`Debug API C#`** : Lance l'API en mode débogage avec support des points d'arrêt et du Hot Reload.
* **`Action: Migrer la BDD`** : Une commande sécurisée qui met à jour ton schéma de base de données avant que tu ne lances ton code.
## 🏗️ Vue d’ensemble du projet

TaskForce est une solution multi-plateforme organisée en **monorepo**. Elle gère l’attribution intelligente de tâches avec une communication temps réel.

- **Backend** : ASP.NET Core (C#), architecture Clean, SignalR, Entity Framework Core, PostgreSQL
- **Frontend** : React (web-app), Astro (landing-page)
- **Desktop** : Tauri (Rust)
- **Orchestration** : Docker Compose

---

## 📂 Structure des dossiers principaux

- `apps/Api.Back/` : API principale, logique métier, routes, modèles, services, validation, hubs SignalR
- `apps/web-app/` : Frontend React, gestion d’état avec Zustand, pages, composants, appels API
- `apps/landing-page/` : Landing page marketing, Astro
- `apps/desktop/` : Application desktop, Tauri (Rust)
- `apps/doc/` : Documentation technique
---

## 🔎 Points d’entrée importants

### Backend (Api.Back)
- `Controllers/` : Définissent les routes de l’API
- `Services/` : Logique métier (attribution, gestion des tâches)
- `Hubs/` : Communication temps réel (SignalR)
- `Data/` : Configuration de la base de données
- `Models/` : Entités de la base
- `DTOs/` : Objets de transfert de données
- `Validators/` : Validation des entrées
- `Shared/` : Constantes, routes partagées

### Frontend (web-app)
- `src/pages/` : Pages principales de l’application
- `src/components/` : Composants réutilisables
- `src/api/` : Fonctions d’appel à l’API
- `src/store/` : Gestion d’état (Zustand)

### Desktop (Tauri)
- `src-tauri/src/` : Code Rust principal
- `src-tauri/tauri.conf.json` : Configuration Tauri

---

## 🔗 Communication entre modules

- **API REST** : Communication entre le frontend, le desktop et l’API
- **SignalR** : Temps réel (notifications, mises à jour de tâches)
- **DTOs** : Format standardisé pour les échanges

---

## 🧩 Comment explorer le code ?

1. **Commence par le dossier `apps/`** : repère les modules principaux.
2. **Lis les fichiers README.md** de chaque module s’ils existent.
3. **Pour le backend** : commence par `Controllers/` pour voir les routes, puis remonte vers les services et modèles.
4. **Pour le frontend** : explore les pages, puis les composants et la gestion d’état.
5. **Pour le desktop** : regarde le point d’entrée Rust et la configuration Tauri.
6. **Utilise la recherche globale** (Ctrl+Maj+F) pour retrouver l’utilisation d’une entité, d’un service ou d’une route.

---

## 🛠️ Bonnes pratiques de navigation

- Prends le temps de lire les commentaires et la documentation dans le code.
- N’hésite pas à poser des questions à l’équipe sur les parties complexes.
- Utilise les outils d’analyse (SonarLint, .NET Analyzers) pour comprendre les alertes éventuelles.
- Consulte le fichier `Architecture.md` pour une vision technique globale.

---

## 🛠️ Utilisation des Scripts PowerShell

Les scripts situés à la racine permettent d'automatiser les tâches de maintenance sans passer par l'interface de VS Code.

### Initialisation complète (`init-dev.ps1`)
À utiliser après un `git pull` ou sur une nouvelle machine (PC B). Il installe pnpm, restaure .NET et crée tes fichiers `.env`.

### Gestion de l'infrastructure (`dev-docker.ps1`)
À utiliser avant de lancer l'API. Il vérifie que Docker est actif et que le port de la base de données est disponible.

> **Note :** Si VS Code refuse de lancer les scripts, exécute `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` dans un terminal PowerShell en mode administrateur.

## 🤝 Ressources utiles

- `doc/Architecture.md` : Architecture technique détaillée
- L’équipe projet !

Bonne exploration et bienvenue sur TaskForce 🚀


Pour installer un package Node dans ton frontend (React/Vite) :
docker exec -it taskforce_webapp sh
# Une fois dedans, tu seras déjà dans le bon dossier (/workspace/apps/web-app)
pnpm add nom-du-package
exit

Pour ajouter un package NuGet dans ton API .NET :

Bash
docker exec -it taskforce_api bash
# Une fois dedans (/workspace/apps/api.back)
dotnet add package Nom.Du.Package
exit

Pour voir les logs en direct (très utile pour voir si ton code compile quand tu le sauvegardes) :

Bash
docker logs -f taskforce_api
# ou
docker logs -f taskforce_webapp