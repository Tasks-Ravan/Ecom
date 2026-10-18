import type { ProblemDetails } from './types';

const baseUrl = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '');
let accessToken: string | null = null;
let refreshToken = localStorage.getItem('ecommerce.refreshToken');
let refreshInFlight: Promise<string | null> | null = null;
let onSessionExpired: (() => void) | null = null;

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly problem: ProblemDetails,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export const setAccessToken = (token: string | null) => {
  accessToken = token;
};

export const setRefreshToken = (token: string | null) => {
  refreshToken = token;
  if (token) localStorage.setItem('ecommerce.refreshToken', token);
  else localStorage.removeItem('ecommerce.refreshToken');
};

export const setSessionExpiredHandler = (handler: (() => void) | null) => {
  onSessionExpired = handler;
};

const parseError = async (response: Response): Promise<ApiError> => {
  let problem: ProblemDetails = {};
  try {
    problem = (await response.json()) as ProblemDetails;
  } catch {
    problem = { title: response.statusText };
  }
  return new ApiError(
    problem.detail || problem.error || problem.title || `Request failed (${response.status})`,
    response.status,
    problem,
  );
};

const requestRefresh = async (): Promise<string | null> => {
  if (!refreshToken) return null;
  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const response = await fetch(`${baseUrl}/auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken }),
        });
        if (!response.ok) return null;
        const result = (await response.json()) as {
          accessToken: string;
          refreshToken: string;
        };
        accessToken = result.accessToken;
        setRefreshToken(result.refreshToken);
        return accessToken;
      } catch {
        return null;
      } finally {
        refreshInFlight = null;
      }
    })();
  }
  return refreshInFlight;
};

export async function apiRequest<T>(path: string, init: RequestInit = {}, retry = true): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body && !(init.body instanceof FormData)) headers.set('Content-Type', 'application/json');
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);
  headers.set('Accept-Language', localStorage.getItem('ecommerce.language') || 'en');

  const response = await fetch(`${baseUrl}${path}`, { ...init, headers });
  if (response.status === 401 && retry && !path.startsWith('/auth/')) {
    const refreshedToken = await requestRefresh();
    if (refreshedToken) return apiRequest<T>(path, init, false);
    setAccessToken(null);
    setRefreshToken(null);
    onSessionExpired?.();
  }
  if (!response.ok) throw await parseError(response);
  if (response.status === 204) return undefined as T;
  const contentType = response.headers.get('content-type') || '';
  if (contentType.includes('application/json')) return (await response.json()) as T;
  return (await response.blob()) as T;
}

export const apiBaseUrl = baseUrl;
