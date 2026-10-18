import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LoadingState } from '../components/States';
import { useAuth } from '../lib/auth-context';

export function ProtectedRoute() {
  const auth = useAuth();
  const location = useLocation();
  if (!auth.isReady) return <div className="route-loading"><LoadingState rows={2} /></div>;
  if (!auth.isAuthenticated) return <Navigate to="/login" replace state={{ from: location }} />;
  return <Outlet />;
}

export function AdminRoute() {
  const auth = useAuth();
  const location = useLocation();
  if (!auth.isReady) return <div className="route-loading"><LoadingState rows={2} /></div>;
  if (!auth.isAuthenticated) return <Navigate to="/login" replace state={{ from: location }} />;
  if (!auth.isAdmin) return <Navigate to="/403" replace />;
  return <Outlet />;
}

export function ForbiddenPage() {
  const { t } = useTranslation();
  return <StatusPage code="403" title={t('errors.forbidden')} />;
}

export function NotFoundPage() {
  const { t } = useTranslation();
  return <StatusPage code="404" title={t('errors.notFound')} />;
}

function StatusPage({ code, title }: { code: string; title: string }) {
  const { t } = useTranslation();
  return <main className="status-page"><span>{code}</span><h1>{title}</h1><a className="button button-primary button-default" href="/">{t('common.back')}</a></main>;
}
