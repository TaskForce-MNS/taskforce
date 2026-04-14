import { create } from 'zustand';
import { startAuthentication, type PublicKeyCredentialRequestOptionsJSON} from '@simplewebauthn/browser';
import { authMe, authn, login, loginOptions, logout } from '@/config/api';
import { apiClient } from '@/api/Client';

interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  loginWithPasskey: () => Promise<void>;
  logout: () => Promise<void>;
  checkSession: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()((set) => ({
  isAuthenticated: false,
  isLoading: true,
  error: null,

  loginWithPasskey: async () => {
    set({ isLoading: true, error: null });
   try {
      const options = await apiClient<PublicKeyCredentialRequestOptionsJSON>(`${authn}${login}${loginOptions}`, {
        method: 'POST',
      });
  const assertionResponse = await startAuthentication({ 
    optionsJSON: options
  });

     await apiClient(`${authn}${login}`, {
        method: 'POST',
        body: JSON.stringify({ WebAuthnAssertionResponse: assertionResponse }),
      });

      set({ isAuthenticated: true, isLoading: false });

    } catch (err) {
      const message = err instanceof Error ? err.message : "Erreur de connexion inconnue.";
      set({ error: message, isLoading: false });
    }
  },

 logout: async () => {
    try {
      // On prévient le backend de détruire le cookie
      await apiClient(`${authn}${logout}`, { method: 'POST' });
    } catch (error) {
      console.warn("Erreur lors de la déconnexion backend, mais on vide le state local.", error);
    } finally {
      set({ isAuthenticated: false });
    }
  },

  // À appeler au démarrage de l'app pour vérifier si une session existe
  checkSession: async () => {
    set({ isLoading: true });
    try {
      await apiClient(`${authn}${authMe}`);
      set({ isAuthenticated: true, isLoading: false });
    } catch {
      set({ isAuthenticated: false, isLoading: false });
    }
  },
}));