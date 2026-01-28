//! Gestion centralisée des erreurs Rust côté Tauri
use tauri::Manager;
use thiserror::Error;

#[derive(Debug, Error)]
pub enum AppError {
    #[error("Erreur de validation : {0}")]
    Validation(String),
    #[error("Accès non autorisé")]
    Unauthorized,
    #[error("Erreur interne : {0}")]
    Internal(String),
}

impl From<anyhow::Error> for AppError {
    fn from(e: anyhow::Error) -> Self {
        AppError::Internal(e.to_string())
    }
}

// Exemple d’utilisation dans une commande Tauri
#[tauri::command]
pub fn do_something() -> Result<String, AppError> {
    // ...
    Err(AppError::Validation("Champ manquant".into()))
}
