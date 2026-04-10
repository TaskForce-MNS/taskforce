import { create } from 'zustand';
import { startAuthentication } from '@simplewebauthn/browser';
import { API_BASE_URL } from '@/config/api';

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
  isLoading: false,
  error: null,

  loginWithPasskey: async () => {
    set({ isLoading: true, error: null });
    try {
      const optionsRes = await fetch(`${API_BASE_URL}/auth/login/options`, {
        method: 'POST',
        credentials: 'include', // 🔑 envoie et reçoit les cookies
        headers: { 'Content-Type': 'application/json' },
      });
      if (!optionsRes.ok) throw new Error("Impossible de récupérer les options.");

      const options = await optionsRes.json();
      const assertionResponse = await startAuthentication(options);

      const loginRes = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        credentials: 'include', // 🔑 le backend pose le cookie ici
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ WebAuthnAssertionResponse: assertionResponse }),
      });
      if (!loginRes.ok) throw new Error("Échec de la validation biométrique.");

      set({ isAuthenticated: true, isLoading: false });

    } catch (err) {
      const message = err instanceof Error ? err.message : "Erreur de connexion inconnue.";
      set({ error: message, isLoading: false });
    }
  },

  logout: async () => {
    // Appel backend pour invalider la session ET effacer le cookie
    await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      credentials: 'include',
    });
    set({ isAuthenticated: false });
  },

  // À appeler au démarrage de l'app pour vérifier si une session existe
  checkSession: async () => {
    set({ isLoading: true });
    try {
      const res = await fetch(`${API_BASE_URL}/auth/me`, {
        credentials: 'include',
      });
      set({ isAuthenticated: res.ok, isLoading: false });
    } catch {
      set({ isAuthenticated: false, isLoading: false });
    }
  },
}));