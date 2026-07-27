import { create } from 'zustand';
import {
  startAuthentication,
  startRegistration,
  type PublicKeyCredentialRequestOptionsJSON,
  type PublicKeyCredentialCreationOptionsJSON
} from '@simplewebauthn/browser';
import { authMe, auth, login, authOptions, logout, register } from '@/api/config';
import { apiClient } from '@/api/client';


export interface User {
  id: string;
  email: string;
  firstname: string;
  lastname: string;
  title: string;
  workload: number;
  experience: string;
  createdAt: string;
  avatarUrl?: string;
}
interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  user: User | null;

  loginWithPasskey: () => Promise<boolean>;
  registerWithPasskey: (profileData: { firstname: string, lastname: string, title: string; experience: string }) => Promise<void>;
  logout: () => Promise<void>;
  checkSession: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()((set, get) => ({
  isAuthenticated: false,
  isLoading: true,
  error: null,
  user: null,

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

      await get().checkSession();
      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : "Erreur de connexion.";
      set({ error: message.includes("operation") ? "L'authentification a été annulée." : message, isLoading: false });
      return false;
    }
  },

  registerWithPasskey: async (profileData) => {
    set({ isLoading: true, error: null });
    try {
      const displayName = `${profileData.firstname} ${profileData.lastname}`;
      const options = await apiClient<PublicKeyCredentialCreationOptionsJSON>(`${auth}${register}${authOptions}?displayName=${encodeURIComponent(displayName)}`, {
        method: 'POST',
      });

      const attestationResponse = await startRegistration({ optionsJSON: options });

      await apiClient(`${auth}${register}`, {
        method: 'POST',
        body: JSON.stringify({
          EncryptedProfileBlob: "dGVtcA==",
          FirstName: profileData.firstname,
          LastName: profileData.lastname,
          Experience: profileData.experience,
          Title: profileData.title,
          WebAuthnAttestationResponse: attestationResponse
        }),
      });

      await get().checkSession();
    } catch (err) {
      const message = err instanceof Error ? err.message : "Erreur lors de l'inscription.";
      set({ error: message.includes("operation") ? "L'inscription a été annulée (capteur biométrique fermé)." : message, isLoading: false });
    }
  },

  logout: async () => {
    try {
      await apiClient(`${auth}${logout}`, { method: 'POST' });
    } catch (error) {
      console.warn("Erreur lors de la déconnexion backend, mais on vide le state local.", error);
    } finally {
      set({ isAuthenticated: false, user: null });
    }
  },

  checkSession: async () => {
    set({ isLoading: true });
    try {

      const userData = await apiClient<User>(`${auth}${authMe}`);
      set({ isAuthenticated: true, user: userData, isLoading: false });
    } catch {
      set({ isAuthenticated: false, user: null, isLoading: false });
    }
  },
}));