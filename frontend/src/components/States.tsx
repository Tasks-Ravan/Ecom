import { AlertCircle, PackageOpen } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { ReactNode } from 'react';
import { Button } from './ui/button';

export function LoadingState({ rows = 4 }: { rows?: number }) {
  const { t } = useTranslation();
  return <div className="space-y-4" aria-label={t('common.loading')}>
    {Array.from({ length: rows }, (_, index) => <div key={index} className="skeleton h-16 rounded-xl" />)}
  </div>;
}

export function EmptyState({ title, action }: { title?: string; action?: ReactNode }) {
  const { t } = useTranslation();
  return <div className="empty-state"><span className="empty-icon"><PackageOpen size={24} /></span>
    <h3>{title || t('common.empty')}</h3>{action && <div className="mt-4">{action}</div>}</div>;
}

export function ErrorState({ message, onRetry }: { message?: string; onRetry: () => void }) {
  const { t } = useTranslation();
  return <div className="error-state" role="alert"><AlertCircle size={22} />
    <span>{message || t('common.error')}</span><Button size="sm" variant="secondary" onClick={onRetry}>{t('common.retry')}</Button></div>;
}
