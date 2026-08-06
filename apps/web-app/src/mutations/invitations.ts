import { useMutation, useQueryClient } from '@tanstack/react-query';
import { invitationsApi, type CreateInvitationPayload } from '@/api/invitations';
import { useToastStore } from '@/stores/useToastStore';

export const useCreateInvitation = (projectId: string) => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (payload: CreateInvitationPayload) => invitationsApi.create(projectId, payload),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['projects', projectId, 'invitations'] });
            addToast({ variant: 'success', title: 'Lien généré', message: "Le lien d'invitation a été créé." });
        },
        onError: (error) => {
            addToast({ variant: 'error', title: 'Erreur', message: error instanceof Error ? error.message : 'Impossible de générer le lien.' });
        },
    });
};

export const useRevokeInvitation = (projectId: string) => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (invitationId: string) => invitationsApi.revoke(invitationId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['projects', projectId, 'invitations'] });
            addToast({ variant: 'info', title: 'Lien révoqué' });
        },
        onError: (error) => {
            addToast({ variant: 'error', title: 'Erreur', message: error instanceof Error ? error.message : 'Impossible de révoquer.' });
        },
    });
};

export const useAcceptInvitation = () => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (token: string) => invitationsApi.accept(token),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['projects'] });
            addToast({ variant: 'success', title: 'Bienvenue !', message: 'Vous avez rejoint le projet.' });
        },
        onError: (error) => {
            addToast({ variant: 'error', title: 'Erreur', message: error instanceof Error ? error.message : 'Impossible de rejoindre le projet.' });
        },
    });
};