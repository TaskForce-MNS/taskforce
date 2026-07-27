import { API_BASE_URL, auth, login, refreshToken } from '@/api/config';
import { useAuthStore } from '@/stores/useAuthStore';

let isRefreshing = false;
let failedQueue: { resolve: (value?: unknown) => void; reject: (reason?: unknown) => void; }[] = [];

const processQueue = (error: unknown = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve();
    }
  });

  failedQueue = [];
}


type ApiClientOptions = Omit<RequestInit, 'body'> & { body?: unknown };

export const apiClient = async <T>(
  endpoint: string,
  options: ApiClientOptions = {}
): Promise<T> => {
  const headers = new Headers(options.headers);

  if (
    !headers.has('Content-Type')
    && options.body
    && !(options.body instanceof FormData)
  ) {
    headers.set('Content-Type', 'application/json');
  }

  let finalBody: BodyInit | null | undefined;
  if (
    options.body &&
    typeof options.body === 'object' &&
    !(options.body instanceof FormData) &&
    !(options.body instanceof Blob)
  ) {
    finalBody = JSON.stringify(options.body);
  } else {
    finalBody = options.body as BodyInit | null | undefined;
  }

  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
  const url = `${API_BASE_URL}${normalizedEndpoint}`;

  let response = await fetch(url, {
    ...options,
    headers,
    body: finalBody,
    credentials: 'include',
  });

  const isAuthRoute = url.includes('/refresh') ||
    url.includes('/logout') ||
    url.includes('/login') ||
    url.includes('/register');

  if (response.status === 401 && !isAuthRoute) {
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then(() => {
        return apiClient<T>(endpoint, options);
      }).catch((err) => {
        return Promise.reject(err);
      });
    }

    isRefreshing = true;
    try {
      const refreshResponse = await fetch(`${API_BASE_URL}${auth}${login}${refreshToken}`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (refreshResponse.ok) {
        processQueue(null);
        response = await fetch(url, {
          ...options,
          headers,
          body: finalBody,
          credentials: 'include',
        });
      } else {
        throw new Error('Refresh token invalide');
      }
    } catch (error) {
      processQueue(error);
      useAuthStore.setState({ isAuthenticated: false, isLoading: false });
      throw new Error('Session expirée. Veuillez vous reconnecter.');
    } finally {
      isRefreshing = false;
    }
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);

    if (response.status === 404) {
      throw new Error('La ressource demandée est introuvable (404).');
    }

    if (errorData && Array.isArray(errorData) && errorData.length > 0 && errorData[0].errorMessage) {
      throw new Error(errorData[0].errorMessage);
    }

    if (errorData?.title) {
      throw new Error(errorData.title);
    }

    if (errorData?.message) {
      throw new Error(errorData.message);
    }

    throw new Error('Une erreur inattendue est survenue.');
  }

  if (response.status === 204) return undefined as T;

  return response.json();
};