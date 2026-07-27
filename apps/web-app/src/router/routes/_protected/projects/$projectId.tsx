import { createFileRoute } from '@tanstack/react-router';
import { projectQueryOptions } from '@/api/queries/projectsQueries';
import { ProjectDetail } from '@/pages/ProjectDetail/ProjectDetail';

export const Route = createFileRoute('/_protected/projects/$projectId')({
  loader: ({ context: { queryClient }, params: { projectId } }) =>
    queryClient.ensureQueryData(projectQueryOptions(projectId)),

  component: function ProjectRouteComponent() {
    const { projectId } = Route.useParams();
    return <ProjectDetail projectId={projectId} />;
  },
});