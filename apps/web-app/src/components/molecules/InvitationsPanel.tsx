import { useQuery } from '@tanstack/react-query';
import { projectInvitationsQueryOptions } from '@/api/queries/invitationsQueries';
import { useCreateInvitation, useRevokeInvitation } from '@/mutations/invitations';
import { Button } from '@/components/atoms/Button';
import { useToastStore } from '@/stores/useToastStore';
import { useState } from 'react';

interface InvitationsPanelProps {
    projectId: string;
}

export const InvitationsPanel = ({ projectId }: InvitationsPanelProps) => {
    const [now] = useState(() => Date.now());
    const { data: invitations, isPending } = useQuery(projectInvitationsQueryOptions(projectId));
    const createMutation = useCreateInvitation(projectId);
    const revokeMutation = useRevokeInvitation(projectId);
    const addToast = useToastStore((state) => state.addToast);

    const handleCreate = () => {
        createMutation.mutate({}); // défauts backend : 7 jours, usages illimités
    };

    const handleCopy = async (token: string) => {
        try {
            const url = `${window.location.origin}/invite/${token}`;
            await navigator.clipboard.writeText(url);
            addToast({ variant: 'success', title: 'Lien copié', duration: 2000 });
        } catch {
            addToast({ variant: 'error', title: 'Erreur lors de la copie', duration: 3000 });
        }
    };

    return (
        <div className="rounded-medium border border-white-accent-dark/10 bg-black-accent-default p-5">
            <div className="flex items-center justify-between">
                <h2 className="font-title text-lg font-semibold text-white-accent-light">
                    Invitations
                </h2>
                <Button variant="primary" size="sm" isLoading={createMutation.isPending} onClick={handleCreate}>
                    + Générer un lien
                </Button>
            </div>

            {isPending ? (
                <p className="mt-4 min-h-10 text-sm text-white-accent-dark">
                    Chargement des invitations...
                </p>
            ) : invitations?.length === 0 ? (
                <p className="mt-4 text-sm italic text-white-accent-dark">
                    Aucune invitation active.
                </p>
            ) : (
                <ul className="mt-4 flex flex-col gap-2">
                    {invitations?.map((invitation) => {
                        const expiresIn = Math.max(
                            0,
                            Math.ceil((new Date(invitation.expiresAt).getTime() - now) / (1000 * 60 * 60 * 24))
                        );
                        const isRevokingThis = revokeMutation.isPending && revokeMutation.variables === invitation.id;

                        return (
                            <li
                                key={invitation.id}
                                className="flex items-center justify-between gap-3 rounded-small border border-white-accent-dark/10 bg-black-accent-light/30 p-3"
                            >
                                <div className="min-w-0 flex-1">
                                    <p className="truncate font-mono text-xs text-white-accent-light">
                                        {invitation.token}
                                    </p>
                                    <p className="mt-0.5 text-[10px] text-white-accent-dark">
                                        Expire dans {expiresIn} jour{expiresIn > 1 ? 's' : ''} ·{' '}
                                        {invitation.usesLeft === null ? 'usages illimités' : `${invitation.usesLeft} usage(s) restant(s)`}
                                    </p>
                                </div>
                                <div className="flex shrink-0 gap-2">
                                    <Button variant="outline" size="sm" onClick={() => handleCopy(invitation.token)}>
                                        Copier
                                    </Button>
                                    <Button
                                        variant="danger"
                                        size="sm"
                                        isLoading={isRevokingThis}
                                        onClick={() => revokeMutation.mutate(invitation.id)}
                                    >
                                        Révoquer
                                    </Button>
                                </div>
                            </li>
                        );
                    })}
                </ul>
            )}
        </div>
    );
};