// api/client.ts
import { API_BASE_URL, auth, login, refreshToken } from '@/api/config';
import { useAuthStore } from '@/stores/useAuthStore';

type ApiClientOptions = Omit<RequestInit, 'body'> & { body?: unknown };

let isRefreshing = false;
let failedQueue: { resolve: (value?: unknown) => void; reject: (reason?: unknown) => void }[] = [];

const processQueue = (error: unknown = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve();
    }
  });
  failedQueue = [];
};

// ────────────────────────────────────────────────────────────
// Helpers extraits — chacun réduit la complexité de apiClient
// ────────────────────────────────────────────────────────────

function buildHeaders(options: ApiClientOptions): Headers {
  const headers = new Headers(options.headers);

  if (
    !headers.has('Content-Type') &&
    options.body &&
    !(options.body instanceof FormData)
  ) {
    headers.set('Content-Type', 'application/json');
  }

  return headers;
}

function buildBody(body: unknown): BodyInit | null | undefined {
  if (
    body &&
    typeof body === 'object' &&
    !(body instanceof FormData) &&
    !(body instanceof Blob)
  ) {
    return JSON.stringify(body);
  }
  return body as BodyInit | null | undefined;
}

function isAuthRoute(url: string): boolean {
  return (
    url.includes('/refresh') ||
    url.includes('/logout') ||
    url.includes('/login') ||
    url.includes('/register')
  );
}

async function extractErrorMessage(response: Response): Promise<string> {
  const errorData = await response.json().catch(() => null);

  if (response.status === 404) {
    return 'La ressource demandée est introuvable (404).';
  }
  if (errorData && Array.isArray(errorData) && errorData.length > 0 && errorData[0].errorMessage) {
    return errorData[0].errorMessage;
  }
  if (errorData?.title) {
    return errorData.title;
  }
  if (errorData?.message) {
    return errorData.message;
  }
  return 'Une erreur inattendue est survenue.';
}

async function refreshSession(
  url: string,
  options: ApiClientOptions,
  headers: Headers,
  finalBody: BodyInit | null | undefined
): Promise<Response> {
  if (isRefreshing) {
    return new Promise((resolve, reject) => {
      failedQueue.push({ resolve, reject });
    })
      .then(() => fetch(url, { ...options, headers, body: finalBody, credentials: 'include' }))
      .catch((err) => Promise.reject(err));
  }

  isRefreshing = true;
  try {
    const refreshResponse = await fetch(`${API_BASE_URL}${auth}${login}${refreshToken}`, {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!refreshResponse.ok) {
      throw new Error('Refresh token invalide');
    }

    processQueue(null);
    return await fetch(url, { ...options, headers, body: finalBody, credentials: 'include' });
  } catch (error) {
    processQueue(error);
    useAuthStore.setState({ isAuthenticated: false, isLoading: false });
    throw new Error('Session expirée. Veuillez vous reconnecter.');
  } finally {
    isRefreshing = false;
  }
}

// ────────────────────────────────────────────────────────────
// Fonction principale — complexité réduite via délégation
// ────────────────────────────────────────────────────────────

export const apiClient = async <T>(
  endpoint: string,
  options: ApiClientOptions = {}
): Promise<T> => {
  const headers = buildHeaders(options);
  const finalBody = buildBody(options.body);
  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
  const url = `${API_BASE_URL}${normalizedEndpoint}`;

  let response = await fetch(url, { ...options, headers, body: finalBody, credentials: 'include' });

  if (response.status === 401 && !isAuthRoute(url)) {
    response = await refreshSession(url, options, headers, finalBody);
  }

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  if (response.status === 204) return undefined as T;

  return response.json();
};