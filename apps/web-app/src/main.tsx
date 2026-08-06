import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from '@tanstack/react-router';
import { QueryClientProvider } from '@tanstack/react-query';
import { router } from '@/router';
import { queryClient } from '@/config/queryClient';
import { useAuthStore } from '@/stores/useAuthStore';
import './index.css';
import '@fontsource/inter';
import '@fontsource/montserrat';
import '@fontsource/plus-jakarta-sans';
import { ToastContainer } from './components/atoms/Toast';

const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Élément root introuvable');

const root = createRoot(rootElement);

// Verify session before rendering the app
useAuthStore.getState().checkSession().finally(() => {
  root.render(
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <ToastContainer />
        <RouterProvider
          router={router}
          context={{
            queryClient
          }}
        />
      </QueryClientProvider>
    </StrictMode>
  );
});