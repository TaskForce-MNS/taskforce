# Backend API (Api.Back)

Ce dossier contient le backend principal de TaskForce.

## Rôle dans l'architecture

- Fournit l’API REST pour toutes les fonctionnalités métier (gestion des tâches, utilisateurs, etc.).
- Implémente la logique métier, la validation, la persistance des données (PostgreSQL via Entity Framework Core).
- Gère la communication temps réel avec les clients (SignalR).
- Sert de point central pour la sécurité, l’authentification et l’intégration avec d’autres modules.

Ce backend est le cœur applicatif : il orchestre les échanges entre la base de données, le frontend, le desktop et les autres services.