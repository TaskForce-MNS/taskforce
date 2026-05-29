import { createFileRoute } from '@tanstack/react-router';
import { dashboardQueryOptions } from '@/api/queries/dashboardQueries';
import { Dashboard } from '@/pages/Dashboard/Dashboard';
import { Alert } from '@/components/atoms/Alert';

export const Route = createFileRoute('/_protected/dashboard')({
  loader: ({ context: { queryClient } }) =>
    queryClient.ensureQueryData(dashboardQueryOptions),

  pendingComponent: () => (
    <div className="flex h-[50vh] items-center justify-center">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary-default border-t-transparent" />
    </div>
  ),

  errorComponent: ({ error }) => (
    <div className="p-4">
      <Alert variant="error" title="Erreur de chargement">
        <p>Impossible de récupérer les données du tableau de bord.</p>
        <p className="mt-2 text-xs font-mono opacity-80">{error.message}</p>
      </Alert>
    </div>
  ),
  // apres rechargement de la page il y a ecran blanc qui apparrait et disparait
  component: Dashboard,
});