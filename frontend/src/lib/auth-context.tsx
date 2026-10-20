import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { login as loginRequest, refreshSession } from '../api/auth';
import { setAccessToken, setRefreshToken, setSessionExpiredHandler } from '../api/client';
import type { AuthResponse, UserRole } from '../api/types';

export interface AuthUser {
  id: string;
  email: string;
  roles: UserRole[];
}

export interface AuthContextValue {
  user: AuthUser | null;
  isReady: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  signIn: (email: string, password: string) => Promise<AuthUser>;
  completeSignIn: (response: AuthResponse) => AuthUser;
  signOut: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

const decodeUser = (token: string): AuthUser => {
  const payload = token.split('.')[1];
  const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
  const decoded = JSON.parse(atob(normalized)) as Record<string, unknown>;
  const roleClaim = decoded.role ?? decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const roles = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [];
  return {
    id: String(decoded.sub ?? decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? ''),
    email: String(decoded.email ?? ''),
    roles: roles.map(String).filter((role): role is UserRole => role === 'Admin' || role === 'Customer'),
  };
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isReady, setIsReady] = useState(false);

  const applyToken = useCallback((accessToken: string, nextRefreshToken: string): AuthUser => {
    setAccessToken(accessToken);
    setRefreshToken(nextRefreshToken);
    setToken(accessToken);
    const decoded = decodeUser(accessToken);
    setUser(decoded);
    return decoded;
  }, []);

  const signOut = useCallback(() => {
    setAccessToken(null);
    setRefreshToken(null);
    setToken(null);
    setUser(null);
  }, []);

  const restoreStarted = useRef(false);
  useEffect(() => {
    setSessionExpiredHandler(signOut);
    if (restoreStarted.current) return () => setSessionExpiredHandler(null);
    restoreStarted.current = true;
    const stored = localStorage.getItem('ecommerce.refreshToken');
    if (!stored) {
      setIsReady(true);
      return () => setSessionExpiredHandler(null);
    }
    void refreshSession(stored)
      .then((result) => applyToken(result.accessToken, result.refreshToken))
      .catch(() => signOut())
      .finally(() => setIsReady(true));
    return () => setSessionExpiredHandler(null);
  }, [applyToken, signOut]);

  const signIn = useCallback(async (email: string, password: string) => {
    const result = await loginRequest({ email, password });
    return applyToken(result.accessToken, result.refreshToken);
  }, [applyToken]);

  const completeSignIn = useCallback((response: AuthResponse) =>
    applyToken(response.accessToken, response.refreshToken), [applyToken]);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isReady,
    isAuthenticated: Boolean(token),
    isAdmin: user?.roles.includes('Admin') ?? false,
    signIn,
    completeSignIn,
    signOut,
  }), [user, isReady, token, signIn, completeSignIn, signOut]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used inside AuthProvider.');
  return context;
};
