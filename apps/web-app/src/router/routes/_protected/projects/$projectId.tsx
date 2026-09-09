import { createFileRoute } from '@tanstack/react-router';
import { projectsQueryOptions, projectQueryOptions, projectMembersQueryOptions } from '@/api/queries/projectsQueries';
import { ProjectDetail } from '@/pages/ProjectDetail/ProjectDetail';

export const Route = createFileRoute('/_protected/projects/$projectId')({
  loader: async ({ context: { queryClient }, params: { projectId } }) => {
    await Promise.all([
      queryClient.ensureQueryData(projectQueryOptions(projectId)),
      queryClient.ensureQueryData(projectsQueryOptions),
      queryClient.ensureQueryData(projectMembersQueryOptions(projectId)),
    ]);
  },

  component: function ProjectRouteComponent() {
    const { projectId } = Route.useParams();
    return <ProjectDetail projectId={projectId} />;
  },
});