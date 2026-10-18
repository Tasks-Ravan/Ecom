import * as DialogPrimitive from '@radix-ui/react-dialog';
import { X } from 'lucide-react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

export const Dialog = DialogPrimitive.Root;
export const DialogTrigger = DialogPrimitive.Trigger;
export const DialogClose = DialogPrimitive.Close;

export function DialogContent({ children, title, description }: { children: ReactNode; title: string; description?: string }) {
  const { t } = useTranslation();
  return <DialogPrimitive.Portal>
    <DialogPrimitive.Overlay className="dialog-overlay" />
    <DialogPrimitive.Content className="dialog-content">
      <div className="dialog-heading">
        <div><DialogPrimitive.Title className="dialog-title">{title}</DialogPrimitive.Title>
          {description && <DialogPrimitive.Description className="muted text-sm mt-1">{description}</DialogPrimitive.Description>}</div>
        <DialogPrimitive.Close className="icon-button" aria-label={t('common.close')}><X size={18} /></DialogPrimitive.Close>
      </div>
      {children}
    </DialogPrimitive.Content>
  </DialogPrimitive.Portal>;
}
