import { apiClient } from '@/api/client';
import { ListProjects, InvitationsBase, AcceptInvitation } from '@/api/config';

export interface Invitation {
    id: string;
    projectId: string;
    token: string;
    expiresAt: string;
    usesLeft: number | null;
    createdAt: string;
}

export interface CreateInvitationPayload {
    expiresInDays?: number;
    usesLeft?: number;
}

export interface AcceptInvitationResponse {
    message: string;
    projectId: string;
}

export const invitationsApi = {
    listForProject: (projectId: string) =>
        apiClient<Invitation[]>(`${ListProjects}/${projectId}/invitations`),

    create: (projectId: string, payload: CreateInvitationPayload) =>
        apiClient<Invitation>(`${ListProjects}/${projectId}/invitations`, {
            method: 'POST',
            body: {
                ExpiresInDays: payload.expiresInDays,
                UsesLeft: payload.usesLeft,
            },
        }),

    revoke: (invitationId: string) =>
        apiClient<void>(`${InvitationsBase}/${invitationId}`, {
            method: 'DELETE',
        }),

    accept: (token: string) =>
        apiClient<AcceptInvitationResponse>(AcceptInvitation, {
            method: 'POST',
            body: { Token: token },
        }),
};