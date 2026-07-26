import { getStoredRefreshToken, setAccessToken, setStoredRefreshToken } from './session';
import type { AuthResponse } from './types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5163';

// Single-flight, shared by every caller (httpClient's mid-request 401 retry, AuthContext's app-load
// bootstrap, and React StrictMode's dev-only double effect-invoke). This matters beyond avoiding
// wasted requests: the backend rotates the refresh token on use and revokes the *entire* session if
// the same (now-stale) token is ever presented twice, so two independent callers racing to redeem the
// same stored token would nuke the session they're both trying to establish.
let inFlight: Promise<AuthResponse | null> | null = null;

export function redeemRefreshToken(): Promise<AuthResponse | null> {
  inFlight ??= performRefresh().finally(() => {
    inFlight = null;
  });
  return inFlight;
}

async function performRefresh(): Promise<AuthResponse | null> {
  const refreshToken = getStoredRefreshToken();
  if (!refreshToken) return null;

  try {
    const response = await fetch(`${BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) return null;

    const data = (await response.json()) as AuthResponse;
    setAccessToken(data.token);
    setStoredRefreshToken(data.refreshToken);
    return data;
  } catch {
    return null;
  }
}
