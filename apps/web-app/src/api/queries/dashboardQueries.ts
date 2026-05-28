import { queryOptions } from '@tanstack/react-query';
import { apiClient } from '@/api/Client';

export const dashboardQueryOptions = queryOptions({
    queryKey: ['dashboard', 'tasks'],
    queryFn: () => apiClient<{ tasks: any[] }>('/tasks'),
});