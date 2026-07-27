import { queryOptions } from '@tanstack/react-query';
import { projectsApi } from '@/api/project';

export const projectsQueryOptions = queryOptions({
    queryKey: ['projects'],
    queryFn: projectsApi.list,
});

export const projectQueryOptions = (id: string) =>
    queryOptions({
        queryKey: ['projects', id],
        queryFn: () => projectsApi.getById(id),
    });