import type { ButtonHTMLAttributes } from 'react';
import { cn } from '../../lib/utils';

type Variant = 'primary' | 'secondary' | 'ghost' | 'danger';
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> { variant?: Variant; size?: 'default' | 'sm' | 'icon' }

export function Button({ className, variant = 'primary', size = 'default', ...props }: ButtonProps) {
  return <button className={cn('button', `button-${variant}`, `button-${size}`, className)} {...props} />;
}
