import { useNavigate } from '@tanstack/react-router';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from '@tanstack/react-form';
import { invitationsApi, type AcceptInvitationResponse } from '@/api/invitations';
import { useToastStore } from '@/stores/useToastStore';
import { Button } from '@/components/atoms/Button';
import { Input } from '../atoms/Input';
import { Clipboard } from '@/components/icons/index';

const extractToken = (text: string) => {
    try {
        const url = new URL(text);
        const parts = url.pathname.split('/');
        return parts[parts.length - 1];
    } catch {
        return text.trim();
    }
};

export const JoinWorkspaceForm = () => {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    const acceptMutation = useMutation<AcceptInvitationResponse, Error, string>({
        mutationFn: (token: string) => invitationsApi.accept(token),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ['projects'] });
            addToast({ variant: 'success', title: 'Bienvenue dans l\'équipe !', duration: 3000 });
            form.reset();

            navigate({
                to: '/projects/$projectId',
                params: { projectId: data.projectId }
            });
        },
        onError: (error: unknown) => {
            addToast({
                variant: 'error',
                title: 'Erreur',
                message: error instanceof Error ? error.message : 'Lien invalide ou expiré.'
            });
        }
    });

    const form = useForm({
        defaultValues: {
            inviteCode: '',
        },
        onSubmit: async ({ value }) => {
            const token = value.inviteCode.trim();
            if (token) {
                acceptMutation.mutate(extractToken(token));
            }
        },
    });

    const handlePasteAndSubmit = async () => {
        try {
            const clipboardText = await navigator.clipboard.readText();
            if (!clipboardText) {
                addToast({ variant: 'error', title: 'Presse-papier vide.' });
                return;
            }

            const token = extractToken(clipboardText);
            form.setFieldValue('inviteCode', token);
            acceptMutation.mutate(token);

        } catch {
            addToast({
                variant: 'error',
                title: 'Action bloquée',
                message: 'Veuillez coller le code manuellement.'
            });
        }
    };

    return (
        <div className="rounded-medium border border-white-accent-dark/10 bg-black-accent-default p-5">
            <h2 className="mb-4 font-title text-lg font-semibold text-white-accent-light">
                Rejoindre un projet
            </h2>

            <form
                onSubmit={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    form.handleSubmit();
                }}
                className="flex flex-col sm:flex-row gap-3"
            >
                <form.Field
                    name="inviteCode"
                    children={(field) => (
                        <div className="flex-1">
                            <Input
                                label="Code ou lien d’invitation"
                                name={field.name}
                                value={field.state.value}
                                onBlur={field.handleBlur}
                                onChange={(e) => field.handleChange(e.target.value)}
                                placeholder="Collez le lien ou le code..."
                                disabled={acceptMutation.isPending}
                            />
                        </div>
                    )}
                />

                <div className="flex shrink-0 gap-2 sm:items-end">
                    <form.Subscribe
                        selector={(state) => [state.canSubmit, state.isSubmitting]}
                        children={([canSubmit, isSubmitting]) => (
                            <Button
                                type="submit"
                                variant="primary"
                                isLoading={acceptMutation.isPending || isSubmitting}
                                disabled={!canSubmit || acceptMutation.isPending}
                            >
                                Rejoindre
                            </Button>
                        )}
                    />

                    <Button
                        type="button"
                        variant="outline"
                        onClick={handlePasteAndSubmit}
                        disabled={acceptMutation.isPending}
                        title="Coller et valider automatiquement"
                    >
                      <Clipboard className="size-6" />
                    </Button>
                </div>
            </form>

            {acceptMutation.isError && (
                <p className="mt-2 text-xs text-red-400">
                    {acceptMutation.error?.message || 'Erreur lors de la validation du code.'}
                </p>
            )}
        </div>
    );
};