import { defineConfig } from 'vitest/config';
import { loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [react()],
    server: {
      host: '0.0.0.0',
      proxy: {
        '/api': env.VITE_DEV_PROXY_TARGET ?? 'http://localhost:8080',
        '/scalar': env.VITE_DEV_PROXY_TARGET ?? 'http://localhost:8080',
        '/hangfire': env.VITE_DEV_PROXY_TARGET ?? 'http://localhost:8080',
        '/openapi': env.VITE_DEV_PROXY_TARGET ?? 'http://localhost:8080',
      },
    },
    build: {
      rollupOptions: {
        output: {
          manualChunks(id) {
            if (id.includes('/node_modules/react/') || id.includes('/node_modules/react-dom/') || id.includes('/node_modules/scheduler/')) return 'react-vendor';
            if (id.includes('/node_modules/@tanstack/')) return 'data-vendor';
            if (id.includes('/node_modules/lucide-react/')) return 'icons';
            return undefined;
          },
        },
      },
    },
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      css: true,
    },
  };
});
