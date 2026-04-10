import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from '@tanstack/react-router';
import { QueryClientProvider } from '@tanstack/react-query';
import { router } from '@/router';
import { queryClient } from '@/config/queryClient';
import { useAuthStore } from '@/stores/useAuthStore';
import './index.css';


const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Élément root introuvable');

const root = createRoot(rootElement);

// Verify session before rendering the app
useAuthStore.getState().checkSession().then(() => {
  root.render(
    <StrictMode>
       <QueryClientProvider client={queryClient}>

      <RouterProvider
          router={router}
          context={{
            queryClient,
            auth: useAuthStore.getState(),
          }}
        />
       </QueryClientProvider>
    </StrictMode>
  );
});