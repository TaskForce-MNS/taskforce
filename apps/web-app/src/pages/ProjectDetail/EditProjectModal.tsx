import { Button } from '@/components/atoms/Button';
import { Input } from '@/components/atoms/Input';
import { Textarea } from '@/components/atoms/Textarea';
import { usePatchProject } from '@/mutations/projects';
import { useForm } from '@tanstack/react-form';
import { InvitationsPanel } from '@/components/molecules/InvitationsPanel';
import type { Project } from '@/api/project';
import { useEffect } from 'react';

interface EditProjectModalProps {
    project: Project;
    onClose: () => void;
    updatedDate: string;
}

export const EditProjectModal = ({
    project,
    onClose,
    updatedDate,
}: EditProjectModalProps) => {
    const updateMutation = usePatchProject(project.id);

    const form = useForm({
        defaultValues: {
            name: project.name,
            description: project.description ?? '',
            colorHex: project.colorHex ?? '#587B7F',
        },
        onSubmit: async ({ value }) => {
            const payload = buildPatchPayload(value, project);

            if (Object.keys(payload).length === 0) {
                onClose();
                return;
            }

            await updateMutation.mutateAsync(payload);
            onClose();
        },
    });

    function buildPatchPayload(
        values: { name: string; description: string; colorHex: string },
        original: Project
    ) {
        const payload: Record<string, string> = {};

        if (values.name !== original.name) {
            payload.name = values.name;
        }

        if (values.description !== (original.description ?? '')) {
            payload.description = values.description;
        }

        if (values.colorHex !== (original.colorHex ?? '')) {
            payload.colorHex = values.colorHex;
        }

        return payload;
    }
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') onClose();
        };
        document.addEventListener('keydown', handleKeyDown);
        return () => document.removeEventListener('keydown', handleKeyDown);
    }, [onClose]);

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">

            <button
                type="button"
                aria-label="Fermer la fenêtre"
                onClick={onClose}
                className="absolute inset-0 z-0 cursor-default bg-black-accent-dark/80 backdrop-blur-sm"
            />

            <div
                role="dialog"
                aria-modal="true"
                aria-labelledby="edit-project-title"
                className="relative z-10 flex shrink-0 min-w-[320px] sm:min-w-[500px] h-auto max-h-[calc(100dvh-2rem)] w-full max-w-xl flex-col gap-6 overflow-y-auto overflow-x-hidden rounded-2xl border border-white-accent-dark/10 bg-black-accent-default p-5 shadow-2xl sm:p-6"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="flex shrink-0 items-center justify-between">
                    <h2
                        id="edit-project-title"
                        className="font-title text-xl font-bold text-white-accent-light"
                    >
                        Paramètres du projet
                    </h2>


                    <Button variant="outline" size="sm" onClick={onClose} aria-label="Fermer">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-5">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" />
                        </svg>
                    </Button>
                </div>
                <span className="text-xs text-white-accent-dark">
                    Dernière mise à jour : {updatedDate}
                </span>
                <form
                    onSubmit={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        form.handleSubmit();
                    }}
                    className="flex flex-col gap-4 rounded-medium border border-white-accent-dark/10 bg-black-accent-default p-5 shadow-sm"
                >
                    <form.Field
                        name="name"
                        validators={{
                            onChange: ({ value }) => {
                                if (!value || !value.trim()) return 'Le nom est requis';
                                if (value.length < 2) return 'Le nom doit faire au moins 2 caractères';
                                if (value.length > 50) return 'Le nom ne peut pas dépasser 50 caractères';
                                return undefined;
                            },
                        }}
                    >
                        {(field) => (
                            <Input
                                label="Nom du projet"
                                name={field.name}
                                value={field.state.value}
                                onChange={(e) => field.handleChange(e.target.value)}
                                onBlur={field.handleBlur}
                                error={
                                    field.state.meta.isTouched
                                        ? field.state.meta.errors[0]
                                        : undefined
                                }
                            />
                        )}
                    </form.Field>

                    <form.Field name="description">
                        {(field) => (
                            <Textarea
                                label="Description"
                                rows={3}
                                name={field.name}
                                value={field.state.value}
                                onBlur={field.handleBlur}
                                onChange={(e) => field.handleChange(e.target.value)}
                            />
                        )}
                    </form.Field>

                    <form.Field name="colorHex">
                        {(field) => (
                            <div className="flex flex-col gap-2">
                                <span className="text-sm font-medium text-white-accent-default">
                                    Couleur
                                </span>

                                <input
                                    type="color"
                                    name={field.name}
                                    value={field.state.value}
                                    onChange={(e) => field.handleChange(e.target.value)}
                                    className="h-10 w-20 cursor-pointer rounded-small border border-white-accent-dark/20 bg-transparent"
                                />
                            </div>
                        )}
                    </form.Field>

                    <form.Subscribe
                        selector={(state) => [
                            state.canSubmit,
                            state.isSubmitting,
                            state.isDirty,
                        ]}
                    >
                        {([canSubmit, isSubmitting, isDirty]) => (
                            <div className="mt-2 flex flex-col gap-3 border-t border-white-accent-dark/10 pt-4 sm:flex-row sm:items-center sm:justify-between">
                                <div>
                                    {isDirty ? (
                                        <span className="text-xs font-semibold text-amber-400">
                                            ● Modifications en cours
                                        </span>
                                    ) : (
                                        <span className="text-xs text-white-accent-dark">
                                            Aucune modification
                                        </span>
                                    )}
                                </div>

                                <Button
                                    type="submit"
                                    isLoading={isSubmitting}
                                    disabled={!canSubmit || !isDirty}
                                >
                                    Sauvegarder
                                </Button>
                            </div>
                        )}
                    </form.Subscribe>
                </form>

                <div className="shrink-0">
                    <InvitationsPanel projectId={project.id} />
                </div>
            </div>
        </div>
    );
};