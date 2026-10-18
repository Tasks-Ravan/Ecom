import * as AlertDialog from '@radix-ui/react-alert-dialog';
import { useTranslation } from 'react-i18next';
import { Button } from './ui/button';

interface ConfirmDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  onConfirm: () => void;
  busy?: boolean;
}

export function ConfirmDialog({ open, onOpenChange, title, description, onConfirm, busy }: ConfirmDialogProps) {
  const { t } = useTranslation();
  return <AlertDialog.Root open={open} onOpenChange={onOpenChange}>
    <AlertDialog.Portal>
      <AlertDialog.Overlay className="dialog-overlay" />
      <AlertDialog.Content className="dialog-content max-w-md">
        <AlertDialog.Title className="dialog-title">{title}</AlertDialog.Title>
        <AlertDialog.Description className="muted mt-2 text-sm">{description}</AlertDialog.Description>
        <div className="flex justify-end gap-3 mt-7">
          <AlertDialog.Cancel asChild><Button variant="secondary">{t('common.cancel')}</Button></AlertDialog.Cancel>
          <AlertDialog.Action asChild><Button variant="danger" disabled={busy} onClick={onConfirm}>{t('common.yesDelete')}</Button></AlertDialog.Action>
        </div>
      </AlertDialog.Content>
    </AlertDialog.Portal>
  </AlertDialog.Root>;
}
