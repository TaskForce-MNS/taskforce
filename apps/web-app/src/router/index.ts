import { createRouter } from '@tanstack/react-router';
import { routeTree } from '@/routeTree.gen';
import { queryClient } from '@/config/queryClient';

export const router = createRouter({
  routeTree,
  // Injecter queryClient dans le contexte de toutes les routes
  context: {
    queryClient,
    auth: undefined!, // sera fourni par le RouterProvider dans main.tsx
  },
  defaultPreload: 'intent',
  defaultPreloadStaleTime: 0,
});

// Typage global du router (obligatoire avec TanStack Router)
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}