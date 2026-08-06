import { queryOptions } from '@tanstack/react-query';
import { invitationsApi } from '@/api/invitations';

export const projectInvitationsQueryOptions = (projectId: string) =>
    queryOptions({
        queryKey: ['projects', projectId, 'invitations'],
        queryFn: () => invitationsApi.listForProject(projectId),
    });