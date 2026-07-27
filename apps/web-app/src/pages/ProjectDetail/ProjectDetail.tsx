import { useEffect } from 'react';
import { useSuspenseQuery } from '@tanstack/react-query';
import { useForm } from '@tanstack/react-form';
import { projectQueryOptions } from '@/api/queries/projectsQueries';
import { usePutProject, usePatchProject } from '@/mutations/projects';
import { Input } from '@/components/atoms/Input';
import { Textarea } from '@/components/atoms/Textarea';
import { Button } from '@/components/atoms/Button';

export const ProjectDetail = ({ projectId }: { projectId: string }) => {
    const { data: project } = useSuspenseQuery(projectQueryOptions(projectId));

    const updateMutation = usePatchProject(projectId);
    const putMutation = usePutProject(projectId);

    // ── TanStack Form ──
    const form = useForm({
        defaultValues: {
            name: project.name,
            description: project.description ?? '',
            colorHex: project.colorHex ?? '#587B7F',
        },
        onSubmit: async ({ value }) => {
            // Par défaut, la soumission globale du form déclenche un PATCH
            const payload = buildPatchPayload(value, project);
            if (Object.keys(payload).length > 0) {
                updateMutation.mutate(payload);
            }
        },
    });

    // 🔁 Synchronisation si TanStack Query recharge le projet en arrière-plan
    useEffect(() => {
        form.reset({
            name: project.name,
            description: project.description ?? '',
            colorHex: project.colorHex ?? '#587B7F',
        });
    }, [project, form]);

    // ── PATCH : n'envoie que les champs modifiés ──
    const handlePatchSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        // On récupère toutes les valeurs actuelles du formulaire
        const currentValues = {
            name: form.getFieldValue('name'),
            description: form.getFieldValue('description'),
            colorHex: form.getFieldValue('colorHex'),
        };

        const payload = buildPatchPayload(currentValues, project);

        if (Object.keys(payload).length === 0) return;
        updateMutation.mutate(payload);
    };

    // ── PUT : envoie TOUJOURS l'objet complet ──
    const handlePutSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        putMutation.mutate({
            name: form.getFieldValue('name'),
            description: form.getFieldValue('description') || null,
            colorHex: form.getFieldValue('colorHex') || null,
            imageUrl: project.imageUrl,
        });
    };

    // Helper de calcul de delta pour le PATCH
    function buildPatchPayload(
        values: { name: string; description: string; colorHex: string },
        original: typeof project
    ) {
        const payload: Record<string, string> = {};
        if (values.name !== original.name) payload.name = values.name;
        if (values.description !== (original.description ?? '')) payload.description = values.description;
        if (values.colorHex !== (original.colorHex ?? '')) payload.colorHex = values.colorHex;
        return payload;
    }

    return (
        <div className="mx-auto w-full max-w-2xl min-w-[320px] shrink-0 space-y-6">

            {/* En-tête */}
            <div className="flex items-center gap-4">
                <div
                    className="flex h-12 w-12 shrink-0 items-center justify-center rounded-medium font-title text-xl font-bold text-white-accent-light"
                    style={{ backgroundColor: project.colorHex ?? '#587B7F' }}
                >
                    {project.name.charAt(0).toUpperCase()}
                </div>
                <div>
                    <h1 className="font-title text-2xl font-bold text-white-accent-light">
                        {project.name}
                    </h1>
                    <p className="text-xs text-white-accent-dark">
                        Créé le {new Date(project.createdAt).toLocaleDateString('fr-FR')} ·
                        Mis à jour le {new Date(project.updatedAt).toLocaleDateString('fr-FR')}
                    </p>
                </div>
            </div>

            {/* Formulaire */}
            <form
                onSubmit={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    form.handleSubmit();
                }}
                className="flex flex-col gap-4 rounded-medium border border-white-accent-dark/10 bg-black-accent-default p-5"
            >
                {/* Nom */}
                <form.Field
                    name="name"
                    validators={{
                        // 💡 Syntax propre en fonction simple pour éviter les erreurs TypeScript
                        onChange: ({ value }) => {
                            if (!value || !value.trim()) return 'Le nom est requis';
                            if (value.length < 2) return 'Le nom doit faire au moins 2 caractères';
                            if (value.length > 50) return 'Le nom ne peut pas dépasser 50 caractères';
                            return undefined;
                        },
                    }}
                    children={(field) => (
                        <Input
                            label="Nom du projet"
                            name={field.name}
                            value={field.state.value}
                            onChange={(e) => field.handleChange(e.target.value)}
                            onBlur={field.handleBlur}
                            error={field.state.meta.isTouched ? field.state.meta.errors[0] : undefined}
                        />
                    )}
                />

                {/* Description */}
                <form.Field
                    name="description"
                    children={(field) => (
                        <Textarea
                            label="Description"
                            rows={3}
                            name={field.name}
                            value={field.state.value}
                            onBlur={field.handleBlur}
                            onChange={(e) => field.handleChange(e.target.value)}
                        />
                    )}
                />

                {/* Couleur */}
                <form.Field
                    name="colorHex"
                    children={(field) => (
                        <div className="flex flex-col gap-2">
                            <span className="text-sm font-medium text-white-accent-default">Couleur</span>
                            <input
                                type="color"
                                name={field.name}
                                value={field.state.value}
                                onChange={(e) => field.handleChange(e.target.value)}
                                className="h-10 w-20 cursor-pointer rounded-small border border-white-accent-dark/20 bg-transparent"
                            />
                        </div>
                    )}
                />

                {/* Boutons de test PATCH / PUT */}
                <div className="flex gap-3 border-t border-white-accent-dark/10 pt-4">
                    <Button
                        type="button"
                        variant="secondary"
                        isLoading={updateMutation.isPending}
                        onClick={handlePatchSubmit}
                    >
                        Enregistrer (PATCH)
                    </Button>
                    <Button
                        type="button"
                        variant="primary"
                        isLoading={putMutation.isPending}
                        onClick={handlePutSubmit}
                    >
                        Remplacer tout (PUT)
                    </Button>
                </div>

                <p className="text-xs text-white-accent-dark">
                    <strong>PATCH</strong> n'envoie que les champs modifiés ·
                    <strong> PUT</strong> envoie toujours l'objet complet.
                </p>

                {/* Bouton global de soumission relié à l'état du formulaire */}
                <form.Subscribe
                    selector={(state) => [state.canSubmit, state.isSubmitting, state.isDirty]}
                    children={([canSubmit, isSubmitting, isDirty]) => (
                        <div className="flex items-center justify-between border-t border-white-accent-dark/10 pt-4">
                            <div>
                                {isDirty ? (
                                    <span className="text-xs font-semibold text-amber-400">● Modifications en cours</span>
                                ) : (
                                    <span className="text-xs text-white-accent-dark">Aucune modification</span>
                                )}
                            </div>
                            <Button type="submit" isLoading={isSubmitting} disabled={!canSubmit || !isDirty}>
                                Sauvegarder (Via Form Submit)
                            </Button>
                        </div>
                    )}
                />
            </form>
        </div>
    );
};