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
    credentials: 'include',
  });

  if (response.status === 401) {
    useAuthStore.setState({ isAuthenticated: false, isLoading: false });
    throw new Error('Session expirée. Veuillez vous reconnecter.');
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);

    if (errorData && Array.isArray(errorData) && errorData.length > 0 && errorData[0].errorMessage) {
      throw new Error(errorData[0].errorMessage);
    }

    if (errorData?.title) {
      throw new Error(errorData.title);
    }
    throw new Error(errorData.message);
  }

  if (response.status === 204) return undefined as T;

  return response.json();
};