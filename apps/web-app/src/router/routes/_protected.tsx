import { createFileRoute, redirect } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { DashboardLayout } from '@/templates/DashboardLayout';

export const Route = createFileRoute('/_protected')({
  beforeLoad: () => {
    // Si l'utilisateur n'est pas connecté, on l'éjecte vers la page de login
    if (!useAuthStore.getState().isAuthenticated) {
      throw redirect({ to: '/auth' });
    }
  },
  component: DashboardLayout,
});