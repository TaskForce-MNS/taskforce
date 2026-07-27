import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
    projectsApi,
    type CreateProjectPayload,
    type PutProjectPayload,
    type PatchProjectPayload,
} from '@/api/project';
import { useToastStore } from '@/stores/useToastStore';

export const useCreateProject = () => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (payload: CreateProjectPayload) => projectsApi.create(payload),

        onSuccess: (newProject) => {
            queryClient.invalidateQueries({ queryKey: ['projects'] });

            addToast({
                variant: 'success',
                title: 'Projet créé',
                message: `"${newProject.name}" a été créé avec succès.`,
            });
        },

        onError: (error) => {
            addToast({
                variant: 'error',
                title: 'Erreur',
                message: error instanceof Error ? error.message : 'Impossible de créer le projet.',
            });
        },
    });
};

export const usePutProject = (id: string) => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (payload: PutProjectPayload) => projectsApi.put(id, payload),
        onSuccess: (updated) => {
            queryClient.invalidateQueries({ queryKey: ['projects'] });
            queryClient.invalidateQueries({ queryKey: ['projects', id] });
            addToast({ variant: 'success', title: 'Projet mis à jour (PUT)', message: `"${updated.name}" a été remplacé entièrement.` });
        },
        onError: (error) => {
            addToast({ variant: 'error', title: 'Erreur PUT', message: error instanceof Error ? error.message : 'Échec de la mise à jour.' });
        },
    });
};

export const usePatchProject = (id: string) => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (payload: PatchProjectPayload) => projectsApi.patch(id, payload),
        onSuccess: (updated) => {
            queryClient.invalidateQueries({ queryKey: ['projects'] });
            queryClient.invalidateQueries({ queryKey: ['projects', id] });

            addToast({ variant: 'success', title: 'Projet mis à jour (PATCH)', message: `"${updated.name}" a été modifié partiellement.` });
        },
        onError: (error) => {
            addToast({ variant: 'error', title: 'Erreur PATCH', message: error instanceof Error ? error.message : 'Échec de la mise à jour.' });
        },
    });
};