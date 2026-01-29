# 🧪 Stratégie de Tests TaskForce

## 📖 Vue d'ensemble
La qualité du code est assurée à trois niveaux pour couvrir l'ensemble du stack (C#, React, Astro).

---

## 🏗️ Tests Backend (.NET 9)
**Dossier :** `apps/api-csharp/Tests`
* **Framework :** xUnit
* **Bibliothèque d'assertions :** FluentAssertions
* **Types de tests :**
    * **Unitaires :** Validation des `Records` et des algorithmes dans `Services/`.
    * **Validation :** Tests exhaustifs des classes dans `Validators/`.
    * **Intégration :** Simulation de requêtes HTTP sur les `Controllers` avec une base de données de test.

---

## ⚛️ Tests Frontend (React)
**Dossier :** `apps/web-app/src/__tests__`
* **Runner :** Vitest
* **Utilitaires :** React Testing Library
* **Focus :**
    * **Composants :** Vérification du rendu et des interactions UI.
    * **State Management :** Validation de la logique des stores **Zustand**.

---

## 🌐 Tests de Bout en Bout (E2E)
**Outil :** Playwright
* **Cible :** `landing-page` et `web-app`.
* **Scénarios :**
    * Navigation critique sur le site Astro.
    * Flux complet : Connexion -> Création de tâche -> Notification SignalR.

---
## 🐳 Tests d'Intégration avec Docker (Testcontainers)

Pour garantir que l'API interagit correctement avec PostgreSQL, nous utilisons **Testcontainers pour .NET**.

### Avantages pour le projet :
* **Réalisme total** : On teste contre un vrai PostgreSQL, pas une simulation.
* **Isolation** : Chaque série de tests démarre une base vierge, évitant que les tests ne se polluent entre eux.
* **Automatisation** : Rien à installer sur le PC ou le serveur de CI ; si Docker est présent, les tests tournent.

### Workflow :
1. Le test démarre -> Docker lance une instance `postgres:latest`.
2. Entity Framework Core applique les migrations sur ce conteneur.
3. Les tests vérifient la logique métier (ex: création de tâche).
4. Le test s'arrête -> Le conteneur est détruit automatiquement.
## 🚀 Automatisation (CI)
Les tests sont exécutés automatiquement à chaque push via **GitHub Actions**. Si un seul test échoue, le déploiement est bloqué.

```bash
# Commandes rapides
pnpm test        # Lance tous les tests du monorepo
dotnet test      # Lance uniquement les tests C#