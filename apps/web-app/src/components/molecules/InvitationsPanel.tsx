import { useState } from 'react';
import { useSuspenseQuery } from '@tanstack/react-query';
import { projectInvitationsQueryOptions } from '@/api/queries/invitationsQueries';
import { useCreateInvitation, useRevokeInvitation } from '@/mutations/invitations';
import { Button } from '@/components/atoms/Button';
import { useToastStore } from '@/stores/useToastStore';

interface InvitationsPanelProps {
    projectId: string;
}

export const InvitationsPanel = ({ projectId }: InvitationsPanelProps) => {
    const [now] = useState(() => Date.now());
    const addToast = useToastStore((state) => state.addToast);

    const { data: invitations } = useSuspenseQuery(projectInvitationsQueryOptions(projectId));

    const createMutation = useCreateInvitation(projectId);
    const revokeMutation = useRevokeInvitation(projectId);

    const handleCreate = () => {
        createMutation.mutate({});
    };

    const handleCopy = async (token: string) => {
        try {
            const url = `${window.location.origin}/invite/${token}`;
            await navigator.clipboard.writeText(url);
            addToast({ variant: 'success', title: 'Lien copié dans le presse-papier', duration: 2000 });
        } catch {
            addToast({ variant: 'error', title: 'Erreur lors de la copie', duration: 3000 });
        }
    };

    return (
        <section className="rounded-xl border border-white-accent-dark/15 bg-black-accent-light/20 p-5 backdrop-blur-sm shadow-sm">

            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                    <h2 className="font-title text-sm font-semibold text-white-accent-light">
                        Liens d'invitation actifs
                    </h2>
                    <p className="mt-0.5 text-xs text-white-accent-dark">
                        Partagez ces liens pour inviter de nouveaux membres dans l'équipe.
                    </p>
                </div>

                <Button
                    variant="primary"
                    size="sm"
                    isLoading={createMutation.isPending}
                    onClick={handleCreate}
                    className="shrink-0"
                >
                    + Générer un lien
                </Button>
            </div>

            <div className="my-4 h-px w-full bg-white-accent-dark/10" />

            {invitations.length === 0 ? (
                <div className="flex items-center justify-center rounded-lg border border-dashed border-white-accent-dark/20 p-6 text-center">
                    <p className="text-xs text-white-accent-dark">
                        Aucune invitation active pour le moment.
                    </p>
                </div>
            ) : (
                <ul className="flex flex-col gap-2">
                    {invitations.map((invitation) => {
                        const expiresIn = Math.max(
                            0,
                            Math.ceil((new Date(invitation.expiresAt).getTime() - now) / (1000 * 60 * 60 * 24))
                        );
                        const isRevokingThis = revokeMutation.isPending && revokeMutation.variables === invitation.id;

                        return (
                            <li
                                key={invitation.id}
                                className="group flex flex-col sm:flex-row sm:items-center justify-between gap-3 rounded-lg border border-white-accent-dark/10 bg-black-accent-default p-3 transition-colors hover:border-white-accent-dark/20"
                            >
                                <div className="min-w-0 flex-1">
                                    <div className="flex min-w-0 items-center gap-2">
                                        <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-emerald-500" />

                                        <p className="min-w-0 truncate font-mono text-xs text-white-accent-light">
                                            {window.location.origin}/invite/
                                            <span className="font-bold text-primary-light">
                                                {invitation.token}
                                            </span>
                                        </p>
                                    </div>

                                    <p className="mt-1 flex items-center gap-2 text-[10px] text-white-accent-dark">
                                        <span>Expire dans {expiresIn} jour{expiresIn > 1 ? 's' : ''}</span>
                                        <span className="h-1 w-1 rounded-full bg-white-accent-dark/40" />
                                        <span>{invitation.usesLeft === null ? 'Usages illimités' : `${invitation.usesLeft} usage(s) restant(s)`}</span>
                                    </p>
                                </div>

                                <div className="flex shrink-0 items-center gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => handleCopy(invitation.token)}
                                        className="h-8 border-white-accent-dark/20 hover:border-white-accent-light hover:bg-white-accent-dark/10"
                                    >
                                        Copier
                                    </Button>
                                    <Button
                                        variant="danger"
                                        size="sm"
                                        isLoading={isRevokingThis}
                                        onClick={() => revokeMutation.mutate(invitation.id)}
                                        className="h-8"
                                    >
                                        Révoquer
                                    </Button>
                                </div>
                            </li>
                        );
                    })}
                </ul>
            )}
        </section>
    );
};