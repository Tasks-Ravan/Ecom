import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'sonner';
import { router } from './routes/router';
import { queryClient } from './lib/query-client';
import { AuthProvider } from './lib/auth-context';
import { ThemeProvider, useTheme } from './lib/theme-context';

export function App() {
  return <ThemeProvider><QueryClientProvider client={queryClient}><AuthProvider>
    <AppContents />
  </AuthProvider></QueryClientProvider></ThemeProvider>;
}

function AppContents() {
  const { theme } = useTheme();
  return <><RouterProvider router={router} /><Toaster theme={theme} position="top-right" richColors closeButton /></>;
}
