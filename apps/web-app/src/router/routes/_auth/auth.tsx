import { createFileRoute, redirect } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Auth } from '@/pages/Authentication/Auth';

export const Route = createFileRoute('/_auth/auth')({
  beforeLoad: () => {
    if (useAuthStore.getState().isAuthenticated) {
      throw redirect({ to: '/dashboard' });
    }
  },
  component: Auth,
});