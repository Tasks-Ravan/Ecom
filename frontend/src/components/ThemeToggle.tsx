import { Moon, Sun } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useTheme } from '../lib/theme-context';

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();
  const { t } = useTranslation();
  return <button className="utility-button" onClick={toggleTheme} aria-label={t('nav.toggleTheme')} title={t('nav.toggleTheme')}>
    {theme === 'dark' ? <Sun size={17} /> : <Moon size={17} />}
  </button>;
}
