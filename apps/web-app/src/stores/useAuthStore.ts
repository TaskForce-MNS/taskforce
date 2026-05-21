import { create } from 'zustand';
import {
  startAuthentication,
  startRegistration,
  type PublicKeyCredentialRequestOptionsJSON,
  type PublicKeyCredentialCreationOptionsJSON
} from '@simplewebauthn/browser';
import { authMe, auth, login, authOptions, logout, register } from '@/config/api';
import { apiClient } from '@/api/Client';

interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  // pendingAttestation: RegistrationResponseJSON | null;
  loginWithPasskey: () => Promise<boolean>;
  registerWithPasskey: (profileData: { firstname: string, lastname: string, title: string; experience: string }) => Promise<void>;
  // startRegistrationStep1: () => Promise<boolean>;
  // finalizeRegistrationStep2: (profileData: { firstname: string, lastname: string, title: string; experience: string }) => Promise<void>;
  logout: () => Promise<void>;
  checkSession: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()((set, get) => ({
  isAuthenticated: false,
  isLoading: true,
  error: null,
  // pendingAttestation: null,

  loginWithPasskey: async () => {
    set({ isLoading: true, error: null });
    try {
      const options = await apiClient<PublicKeyCredentialRequestOptionsJSON>(`${auth}${login}${authOptions}`, {
        method: 'POST',
      });
      const assertionResponse = await startAuthentication({
        optionsJSON: options
      });

      await apiClient(`${auth}${login}`, {
        method: 'POST',
        body: JSON.stringify({ WebAuthnAssertionResponse: assertionResponse }),
      });

      set({ isAuthenticated: true, isLoading: false });
      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : "Erreur de connexion.";
      if (message.includes("operation")) {
        set({ error: "L'authentification a été annulée.", isLoading: false });
      } else {
        set({ error: message, isLoading: false });
      }
      return false;
    }
  },
  registerWithPasskey: async (profileData) => {
    set({ isLoading: true, error: null });
    try {
      // 1. On demande les options au serveur
      const options = await apiClient<PublicKeyCredentialCreationOptionsJSON>(`${auth}${register}${authOptions}`, {
        method: 'POST',
      });

      // 2. On déclenche le capteur biométrique
      const attestationResponse = await startRegistration({ optionsJSON: options });

      // 3. On envoie TOUT (Profil + Empreinte) au serveur en une seule fois !
      await apiClient(`${auth}${register}`, {
        method: 'POST',
        body: JSON.stringify({
          EncryptedProfileBlob: "dGVtcA==", // Ton mock actuel
          FirstName: profileData.firstname,
          LastName: profileData.lastname,
          Experience: profileData.experience,
          Title: profileData.title,
          WebAuthnAttestationResponse: attestationResponse
        }),
      });

      set({ isAuthenticated: true, isLoading: false });

    } catch (err) {
      const message = err instanceof Error ? err.message : "Erreur lors de l'inscription.";
      if (message.includes("operation")) {
        set({ error: "L'inscription a été annulée (capteur biométrique fermé).", isLoading: false });
      } else {
        set({ error: message, isLoading: false });
      }
    }
  },
  // registerWithPasskey: async () => {
  //   set({ isLoading: true, error: null });
  //   try {
  //     const options = await apiClient<PublicKeyCredentialCreationOptionsJSON>(`${auth}${register}${authOptions}`, {
  //       method: 'POST',
  //     });

  //     const attestationResponse = await startRegistration({
  //       optionsJSON: options
  //     });

  //     await apiClient(`${auth}${register}`, {
  //       method: 'POST',
  //       body: JSON.stringify({ WebAuthnAttestationResponse: attestationResponse }),
  //     });

  //     set({ isAuthenticated: true, isLoading: false });

  //   } catch (err) {
  //     const message = err instanceof Error ? err.message : "Erreur lors de l'inscription.";
  //     if (message.includes("cancelled")) {
  //       set({ error: "L'authentification a été annulée.", isLoading: false });
  //     } else {
  //       set({ error: message, isLoading: false });
  //     }
  //   }
  // },
  // startRegistrationStep1: async () => {
  //   set({ isLoading: true, error: null });
  //   try {
  //     const options = await apiClient<PublicKeyCredentialCreationOptionsJSON>(`${auth}${register}${authOptions}`, {
  //       method: 'POST',
  //     });

  //     const attestationResponse = await startRegistration({ optionsJSON: options });

  //     set({ pendingAttestation: attestationResponse, isLoading: false });
  //     return true;

  //   } catch (err) {
  //     const message = err instanceof Error ? err.message : "Erreur d'inscription.";
  //     if (message.includes("operation")) {
  //       set({ error: "L'inscription a été annulée.", isLoading: false });
  //     } else {
  //       set({ error: message, isLoading: false });
  //     }
  //     return false;
  //   }
  // },
  // finalizeRegistrationStep2: async (profileData) => {
  //   set({ isLoading: true, error: null });
  //   try {
  //     const { pendingAttestation } = get();

  //     if (!pendingAttestation) throw new Error("Passkey manquant. Veuillez recommencer.");

  //     await apiClient(`${auth}${register}`, {
  //       method: 'POST',
  //       body: JSON.stringify({
  //         EncryptedProfileBlob: "dGVtcA==",
  //         FirstName: profileData.firstname,
  //         LastName: profileData.lastname,
  //         Experience: profileData.experience,
  //         Title: profileData.title,
  //         WebAuthnAttestationResponse: pendingAttestation
  //       }),
  //     });

  //     set({ isAuthenticated: true, pendingAttestation: null, isLoading: false });

  //   } catch (err) {
  //     const message = err instanceof Error ? err.message : "Erreur lors de la création du profil.";
  //     set({ error: message, isLoading: false });
  //   }
  // },
  logout: async () => {
    try {
      // On prévient le backend de détruire le cookie
      await apiClient(`${auth}${logout}`, { method: 'POST' });
    } catch (error) {
      console.warn("Erreur lors de la déconnexion backend, mais on vide le state local.", error);
    } finally {
      set({ isAuthenticated: false });
    }
  },

  checkSession: async () => {
    set({ isLoading: true });
    try {
      await apiClient(`${auth}${authMe}`);
      set({ isAuthenticated: true, isLoading: false });
    } catch {
      set({ isAuthenticated: false, isLoading: false });
    }
  },
}));