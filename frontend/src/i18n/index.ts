import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import { resources } from './resources';

const savedLanguage = localStorage.getItem('ecommerce.language');
void i18n.use(initReactI18next).init({
  resources,
  lng: savedLanguage === 'az' || savedLanguage === 'ru' ? savedLanguage : 'en',
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
});
i18n.on('languageChanged', (language) => {
  localStorage.setItem('ecommerce.language', language);
  document.documentElement.lang = language;
});

export default i18n;
