# Tauri + Vanilla TS

This template should help get you started developing with Tauri in vanilla HTML, CSS and Typescript.

## Recommended IDE Setup

- [VS Code](https://code.visualstudio.com/) + [Tauri](https://marketplace.visualstudio.com/items?itemName=tauri-apps.tauri-vscode) + [rust-analyzer](https://marketplace.visualstudio.com/items?itemName=rust-lang.rust-analyzer)

# Application Desktop (Tauri)

Ce dossier contient l’application desktop native de TaskForce.

## Rôle dans l'architecture

- Offre une expérience native sur Windows/Linux/Mac grâce à Tauri (Rust + webview).
- Permet d’accéder à TaskForce sans navigateur, avec des fonctionnalités desktop avancées (notifications, accès fichiers, etc.).
- Interagit avec l’API backend pour la synchronisation des données et la gestion des tâches.
- Peut intégrer des fonctionnalités spécifiques au poste de travail.

L’application desktop vise les utilisateurs qui préfèrent une expérience hors navigateur ou ont des besoins avancés.
