# 🔐 Gestion des Variables d'Environnement (.env)

## 📖 Pourquoi utiliser des fichiers .env ?
Les fichiers `.env` permettent de séparer le **code** des **configurations**. Cela est crucial pour la sécurité : on ne publie jamais ses mots de passe ou ses clés d'API sur GitHub. 

Dans ton stack complexe (C#, React, Python), ces fichiers servent de "tableau de bord" pour changer le comportement de l'app sans toucher au code.

---

## 📂 Le Trio des fichiers Environnement

| Fichier | Rôle | Doit-on le commit (Git) ? |
| :--- | :--- | :--- |
| **`.env.example`** | Le **modèle**. Il liste toutes les clés nécessaires (ex: `DB_PASSWORD=`) sans les valeurs réelles. | **OUI** (pour aider les autres). |
| **`.env.dev`** | Les valeurs pour ton **PC local** (ex: `localhost`, `user_dev`). | **NON** (généralement ignoré). |
| **`.env.prod`** | Les valeurs réelles de **production** (clés secrètes, serveurs réels). | **SURTOUT PAS** (Risque de sécurité). |

---

## 🛠️ Application à ton Monorepo

Puisque tu as une structure organisée, voici comment ces fichiers vont être répartis :

### 1. À la Racine (Root)
Sert principalement à **Docker**.
* Exemple : Définir le port de PostgreSQL ou le mot de passe du conteneur.

### 2. Dans `apps/web-app/` (React)
Vite utilise ces fichiers pour savoir quelle API appeler.
* `VITE_API_URL=http://localhost:5000` (en dev).
* `VITE_API_URL=https://api.taskforce.com` (en prod).

### 3. Dans `apps/Api.Back/`
Bien que .NET utilise `appsettings.json`, les variables d'environnement sont prioritaires en entreprise pour :
* La chaîne de connexion à la base de données.
* Les secrets pour les jetons de connexion (JWT).

---

## ⚠️ La règle d'or : Le `.gitignore`

Pour ne pas faire d'erreur, ton fichier `.gitignore` à la racine doit contenir ces lignes :
```text
.env
.env.dev
.env.prod
!.env.example