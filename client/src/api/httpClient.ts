import { clearStoredRefreshToken, getAccessToken, setAccessToken } from '../auth/session';
import { redeemRefreshToken } from '../auth/refreshCoordinator';
import { emitSessionExpired } from './authEvents';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5163';

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

// The backend's error envelope isn't fully consistent across modules — most
// endpoints send `{ error: string }`, a couple send `{ errors: string[] }`.
// Check both rather than guessing which one a given endpoint uses.
function extractErrorMessage(data: unknown): string | undefined {
  if (data === null || typeof data !== 'object') return undefined;
  const body = data as { error?: unknown; errors?: unknown };
  if (Array.isArray(body.errors)) return body.errors.join(', ');
  if (typeof body.error === 'string') return body.error;
  return undefined;
}

// Some responses carry no envelope at all (403 from role-based auth, 429 from the rate limiter) —
// fall back to a message that actually explains the status rather than a generic "Request failed."
function fallbackMessageFor(status: number): string {
  switch (status) {
    case 403:
      return "You don't have permission to do that.";
    case 429:
      return 'Too many attempts. Please wait a moment and try again.';
    case 500:
    case 502:
    case 503:
      return 'Something went wrong on our end. Please try again shortly.';
    default:
      return 'Request failed.';
  }
}

interface RequestOptions {
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  signal?: AbortSignal;
  /** Attach the current access token as a Bearer header and allow a silent refresh on 401. Defaults to true. */
  auth?: boolean;
}

function handleSessionExpired(): void {
  setAccessToken(null);
  clearStoredRefreshToken();
  emitSessionExpired();
}

async function parseJson(response: Response): Promise<unknown> {
  if (response.status === 204) return null;
  const text = await response.text();
  if (!text) return null;
  return JSON.parse(text) as unknown;
}

async function request<TResponse>(path: string, options: RequestOptions): Promise<TResponse> {
  const { method, body, signal, auth = true } = options;

  const doFetch = async (): Promise<Response> => {
    const headers: Record<string, string> = {};
    if (body !== undefined) headers['Content-Type'] = 'application/json';
    if (auth) {
      const token = getAccessToken();
      if (token) headers.Authorization = `Bearer ${token}`;
    }

    return fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal,
    });
  };

  let response = await doFetch();

  if (response.status === 401 && auth) {
    const refreshed = await redeemRefreshToken();
    if (refreshed) {
      response = await doFetch();
    } else {
      handleSessionExpired();
    }
  }

  const data = await parseJson(response).catch(() => null);

  if (!response.ok) {
    throw new ApiError(extractErrorMessage(data) ?? fallbackMessageFor(response.status), response.status);
  }

  return data as TResponse;
}

export function getJson<TResponse>(path: string, signal?: AbortSignal): Promise<TResponse> {
  return request<TResponse>(path, { method: 'GET', signal });
}

export function postJson<TResponse>(path: string, body: unknown, signal?: AbortSignal): Promise<TResponse> {
  return request<TResponse>(path, { method: 'POST', body, signal });
}

export function putJson<TResponse>(path: string, body: unknown, signal?: AbortSignal): Promise<TResponse> {
  return request<TResponse>(path, { method: 'PUT', body, signal });
}

export function deleteJson<TResponse = void>(path: string, signal?: AbortSignal): Promise<TResponse> {
  return request<TResponse>(path, { method: 'DELETE', signal });
}

/** Unauthenticated variant for endpoints called before an access token exists (login/register/refresh). */
export function postJsonPublic<TResponse>(path: string, body: unknown, signal?: AbortSignal): Promise<TResponse> {
  return request<TResponse>(path, { method: 'POST', body, signal, auth: false });
}
