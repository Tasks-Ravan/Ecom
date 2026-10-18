import * as Select from '@radix-ui/react-select';
import { Check, ChevronDown, Languages } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { Language } from '../api/types';

const choices: { value: Language; label: string }[] = [
  { value: 'az', label: 'AZ' }, { value: 'en', label: 'EN' }, { value: 'ru', label: 'RU' },
];

export function LanguageSwitcher() {
  const { i18n, t } = useTranslation();
  return <Select.Root value={i18n.language} onValueChange={(value) => void i18n.changeLanguage(value)}>
    <Select.Trigger className="utility-button" aria-label={t('nav.language')}><Languages size={16} /><Select.Value />
      <ChevronDown size={14} /></Select.Trigger>
    <Select.Portal><Select.Content className="select-content" position="popper"><Select.Viewport>
      {choices.map((choice) => <Select.Item value={choice.value} key={choice.value} className="select-item">
        <Select.ItemText>{choice.label}</Select.ItemText><Select.ItemIndicator><Check size={14} /></Select.ItemIndicator>
      </Select.Item>)}
    </Select.Viewport></Select.Content></Select.Portal>
  </Select.Root>;
}
