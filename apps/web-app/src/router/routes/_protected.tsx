import { createFileRoute, redirect, Outlet } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';

export const Route = createFileRoute('/_protected')({
  beforeLoad: () => {
    if (!useAuthStore.getState().isAuthenticated) {
      throw redirect({ to: '/login' });
    }
  },
  component: () => <Outlet />,
});