import { queryOptions } from '@tanstack/react-query';
import { tasksApi } from '@/api/task';

export const projectTasksQueryOptions = (projectId: string) =>
    queryOptions({
        queryKey: ['projects', projectId, 'tasks'],
        queryFn: () => tasksApi.listForProject(projectId),
        enabled: !!projectId,
    });