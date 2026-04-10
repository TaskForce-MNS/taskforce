import { createRootRouteWithContext, Outlet } from '@tanstack/react-router';
import { QueryClient } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/useAuthStore';

interface RouterContext {
  queryClient: QueryClient;
  auth: ReturnType<typeof useAuthStore.getState>;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: () => <Outlet />,
});