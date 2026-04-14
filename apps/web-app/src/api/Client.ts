import { API_BASE_URL } from '@/config/api';
import { useAuthStore } from '@/stores/useAuthStore';

export const apiClient = async <T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> => {
  const headers = new Headers(options.headers);

  if (
    !headers.has('Content-Type') &&
    options.body &&
    !(options.body instanceof FormData)
  ) {
    headers.set('Content-Type', 'application/json');
  }

  let body = options.body;
  if (
    body &&
    typeof body === 'object' &&
    !(body instanceof FormData) &&
    !(body instanceof Blob)
  ) {
    body = JSON.stringify(body);
  }

  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;

  const response = await fetch(`${API_BASE_URL}${normalizedEndpoint}`, {
    ...options,
    headers,
    body,
    credentials: 'include', // 🔑 cookie envoyé automatiquement à chaque requête
  });

  if (response.status === 401) {
    // Cookie invalide/expiré → on remet l'état local à zéro
    useAuthStore.setState({ isAuthenticated: false, isLoading: false });
    throw new Error('Session expirée. Veuillez vous reconnecter.');
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || `Erreur HTTP ${response.status}`);
  }

  if (response.status === 204) return undefined as T;

  return response.json();
};