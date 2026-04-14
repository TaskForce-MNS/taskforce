import { createFileRoute, redirect } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Login } from '@/pages/Login';

export const Route = createFileRoute('/_auth/login')({
  beforeLoad: () => {
    if (useAuthStore.getState().isAuthenticated) {
      throw redirect({ to: '/dashboard' });
    }
  },
  component: Login,
});