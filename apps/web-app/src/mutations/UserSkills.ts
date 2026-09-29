import { useMutation } from '@tanstack/react-query';
import { apiClient } from '@/api/client';
import { useAuthStore, ExperienceLevel } from '@/stores/useAuthStore';
import { useToastStore } from '@/stores/useToastStore';

interface AddSkillPayload {
    domain: string;
    level: ExperienceLevel;
}

export const UserSkills = () => {
    const checkSession = useAuthStore((state) => state.checkSession);
    const addToast = useToastStore((state) => state.addToast);

    const addSkill = useMutation({
        mutationFn: (payload: AddSkillPayload) =>
            apiClient('/users/me/skills', { method: 'POST', body: payload }),
        onSuccess: async () => {
            await checkSession();
            addToast({ variant: 'success', title: 'Compétence ajoutée', message: '' });
        },
        onError: () => {
            addToast({ variant: 'error', title: 'Erreur', message: 'Impossible d\'ajouter la compétence' });
        }
    });

    const removeSkill = useMutation({
        mutationFn: (skillId: string) =>
            apiClient(`/users/me/skills/${skillId}`, { method: 'DELETE' }),
        onSuccess: async () => {
            await checkSession();
            addToast({ variant: 'success', title: 'Compétence retirée', message: '' });
        },
        onError: () => {
            addToast({ variant: 'error', title: 'Erreur', message: 'Impossible de retirer la compétence' });
        }
    });

    return { addSkill, removeSkill };
};