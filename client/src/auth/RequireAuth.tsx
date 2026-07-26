import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { Spinner } from '../components/ui/Feedback';

export function RequireAuth({ children }: { children: ReactNode }) {
  const { status, isAuthenticated } = useAuth();
  const location = useLocation();

  if (status === 'loading') {
    return <Spinner label="Checking your session…" />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <>{children}</>;
}

export function RequireRole({ roles, children }: { roles: string[]; children: ReactNode }) {
  const { status, isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (status === 'loading') {
    return <Spinner label="Checking your session…" />;
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  const hasRole = user.roles.some((role) => roles.includes(role));
  if (!hasRole) {
    return <Navigate to="/403" replace />;
  }

  return <>{children}</>;
}
