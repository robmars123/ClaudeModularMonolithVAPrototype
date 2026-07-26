import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import * as authApi from './authApi';
import { onSessionExpired } from '../api/authEvents';
import { redeemRefreshToken } from './refreshCoordinator';
import {
  clearStoredRefreshToken,
  getAccessToken,
  getStoredRefreshToken,
  setAccessToken,
  setStoredRefreshToken,
  subscribeAccessToken,
} from './session';
import type { AuthResponse, AuthUser } from './types';

interface AuthContextValue {
  accessToken: string | null;
  user: AuthUser | null;
  /** 'loading' while a stored refresh token is being redeemed on app start. */
  status: 'loading' | 'ready';
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  /** Applies a freshly-issued AuthResponse (e.g. after self-registration) as the active session. */
  applySession: (response: AuthResponse) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function toUser(response: AuthResponse): AuthUser {
  return { email: response.email, fullName: response.fullName, roles: response.roles };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessTokenState] = useState<string | null>(getAccessToken());
  const [user, setUser] = useState<AuthUser | null>(null);
  const [status, setStatus] = useState<'loading' | 'ready'>('loading');

  const applySession = useCallback((response: AuthResponse) => {
    setAccessToken(response.token);
    setStoredRefreshToken(response.refreshToken);
    setUser(toUser(response));
  }, []);

  const clearSession = useCallback(() => {
    setAccessToken(null);
    clearStoredRefreshToken();
    setUser(null);
  }, []);

  useEffect(() => subscribeAccessToken(setAccessTokenState), []);

  useEffect(() => onSessionExpired(() => setUser(null)), []);

  useEffect(() => {
    let cancelled = false;

    async function bootstrap() {
      const refreshToken = getStoredRefreshToken();
      if (!refreshToken) {
        setStatus('ready');
        return;
      }
      // Goes through the same single-flight coordinator httpClient uses for its 401 retry — see
      // refreshCoordinator.ts for why two independent redemptions of the same stored token must never
      // happen (the backend treats a replayed refresh token as theft and revokes the whole session).
      const response = await redeemRefreshToken();
      if (!cancelled) {
        if (response) applySession(response);
        else clearSession();
        setStatus('ready');
      }
    }

    void bootstrap();
    return () => {
      cancelled = true;
    };
  }, [applySession, clearSession]);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.login({ email, password });
      applySession(response);
    },
    [applySession],
  );

  const logout = useCallback(async () => {
    const refreshToken = getStoredRefreshToken();
    clearSession();
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken);
      } catch {
        // Best-effort: local state is already cleared regardless of API outcome.
      }
    }
  }, [clearSession]);

  const value: AuthContextValue = {
    accessToken,
    user,
    status,
    isAuthenticated: user !== null,
    login,
    logout,
    applySession,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
}
