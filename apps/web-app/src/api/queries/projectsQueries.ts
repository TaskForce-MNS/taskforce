import { queryOptions } from '@tanstack/react-query';
import { membersApi, projectsApi } from '@/api/project';

export const projectsQueryOptions = queryOptions({
    queryKey: ['projects'],
    queryFn: projectsApi.list,
});

export const projectQueryOptions = (id: string) =>
    queryOptions({
        queryKey: ['projects', id],
        queryFn: () => projectsApi.getById(id),
    });

export const projectMembersQueryOptions = (projectId: string) =>
    queryOptions({
        queryKey: ['projects', projectId, 'members'],
        queryFn: () => membersApi.listMembersProject(projectId),
        enabled: !!projectId
    });