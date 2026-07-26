// The access token lives only in memory (module-level state), never in
// localStorage — closing the tab discards it. httpClient reads/writes it
// here directly (avoids a circular import with AuthContext); AuthContext
// subscribes so its React state — and anything rendering from it, like the
// nav header — stays in sync when httpClient silently refreshes it.
type AccessTokenListener = (token: string | null) => void;

let accessToken: string | null = null;
const listeners = new Set<AccessTokenListener>();

export function getAccessToken(): string | null {
  return accessToken;
}

export function setAccessToken(token: string | null): void {
  accessToken = token;
  for (const listener of listeners) listener(token);
}

export function subscribeAccessToken(listener: AccessTokenListener): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

const REFRESH_TOKEN_STORAGE_KEY = 'ecommerce.refreshToken';

export function getStoredRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY);
}

export function setStoredRefreshToken(token: string): void {
  localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, token);
}

export function clearStoredRefreshToken(): void {
  localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY);
}
