import { createFileRoute } from '@tanstack/react-router';
import { queryOptions, useSuspenseQuery } from '@tanstack/react-query';
import { apiClient } from '@/api/Client';

// Définir les queryOptions en dehors du composant (réutilisable + preload)
const dashboardQueryOptions = queryOptions({
  queryKey: ['dashboard'],
  queryFn: () => apiClient<{ tasks: unknown[] }>('/tasks'),
});

export const Route = createFileRoute('/_protected/dashboard')({
  // Preload les données avant le rendu
  loader: ({ context: { queryClient } }) =>
    queryClient.ensureQueryData(dashboardQueryOptions),
  component: DashboardPage,
});

function DashboardPage() {
  const { data } = useSuspenseQuery(dashboardQueryOptions);
  return <pre>{JSON.stringify(data, null, 2)}</pre>;
}