import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'sonner';
import { router } from './routes/router';
import { queryClient } from './lib/query-client';
import { AuthProvider } from './lib/auth-context';
import { ThemeProvider, useTheme } from './lib/theme-context';
import { CartProvider } from './lib/cart-context';

export function App() {
  return <ThemeProvider><QueryClientProvider client={queryClient}><CartProvider><AuthProvider>
    <AppContents />
  </AuthProvider></CartProvider></QueryClientProvider></ThemeProvider>;
}

function AppContents() {
  const { theme } = useTheme();
  return <><RouterProvider router={router} /><Toaster theme={theme} position="top-right" richColors closeButton /></>;
}
