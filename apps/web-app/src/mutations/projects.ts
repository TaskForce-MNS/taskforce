import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
    projectsApi,
    type CreateProjectPayload,
    type PutProjectPayload,
    type PatchProjectPayload,
} from '@/api/project';
import { useToastStore } from '@/stores/useToastStore';

// export const useCreateProject = () => {
//     const queryClient = useQueryClient();
//     const addToast = useToastStore((state) => state.addToast);

//     return useMutation({
//         mutationFn: (payload: CreateProjectPayload) => projectsApi.create(payload),

//         onSuccess: (newProject) => {
//             queryClient.invalidateQueries({ queryKey: ['projects'] });

//             addToast({
//                 variant: 'success',
//                 title: 'Projet créé',
//                 message: `"${newProject.name}" a été créé avec succès.`,
//             });
//         },

//         onError: (error) => {
//             addToast({
//                 variant: 'error',
//                 title: 'Erreur',
//                 message: error instanceof Error ? error.message : 'Impossible de créer le projet.',
//             });
//         },
//     });
// };
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

        // 🌟 On type en 'any' temporairement car la structure de l'erreur 
        // dépend de comment ton `apiClient` gère les rejets (Axios ou Fetch)
        onError: (error: any) => {
            // 1. On cherche l'URL Stripe (la casse dépend de la sérialisation C#, souvent transformée en camelCase)
            const checkoutUrl = error?.response?.data?.checkoutUrl // Si tu utilises Axios
                || error?.data?.checkoutUrl           // Autre structure courante
                || error?.checkoutUrl;                // Si tu l'as mappée directement

            // 2. Si on trouve une URL, c'est un 402 !
            if (checkoutUrl) {
                addToast({
                    variant: 'info',
                    title: 'Limite atteinte',
                    message: 'Redirection vers la page de paiement sécurisée...',
                });

                // 🚀 Redirection magique vers Stripe
                window.location.href = checkoutUrl;
                return; // On arrête là pour ne pas afficher le message d'erreur classique
            }

            // 3. Erreur classique (400, 500, etc.)
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
            addToast({ variant: 'success', title: 'Projet mis à jour', message: `"${updated.name}" a été modifié.` });
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

            addToast({ variant: 'success', title: 'Projet mis à jour', message: `"${updated.name}" a été modifié` });
        },
        onError: (error) => {
            addToast({ variant: 'error', title: 'Erreur PATCH', message: error instanceof Error ? error.message : 'Échec de la mise à jour.' });
        },
    });
};