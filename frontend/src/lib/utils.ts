import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export const cn = (...values: ClassValue[]) => twMerge(clsx(values));

export const formatCurrency = (value: number, language: string) =>
  new Intl.NumberFormat(language, { style: 'currency', currency: 'AZN', maximumFractionDigits: 2 }).format(value);

export const formatDate = (value: string, language: string) =>
  new Intl.DateTimeFormat(language, { dateStyle: 'medium' }).format(new Date(value));

export const safeImageUrl = (value: string | null | undefined) => {
  if (!value) return null;
  try {
    const url = new URL(value);
    return url.protocol === 'https:' || url.protocol === 'http:' ? url.toString() : null;
  } catch {
    return null;
  }
};
