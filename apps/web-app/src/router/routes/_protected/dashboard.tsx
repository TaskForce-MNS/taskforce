import { createFileRoute } from '@tanstack/react-router';
import { projectsQueryOptions } from '@/api/queries/projectsQueries';
import { Dashboard } from '@/pages/Dashboard/Dashboard';

export const Route = createFileRoute('/_protected/dashboard')({
  loader: ({ context: { queryClient } }) =>
    queryClient.ensureQueryData(projectsQueryOptions),
  component: Dashboard,
});