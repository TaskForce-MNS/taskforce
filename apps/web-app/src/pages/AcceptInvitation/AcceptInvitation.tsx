import { useEffect, useRef } from 'react';
import { useParams, useNavigate } from '@tanstack/react-router';
import { useAcceptInvitation } from '@/mutations/invitations';
import { Alert } from '@/components/atoms/Alert';

export const AcceptInvitation = () => {
    const { token } = useParams({ from: '/_protected/invite/$token' });
    const navigate = useNavigate();
    const { mutate, isPending, isError, error, isSuccess, data } = useAcceptInvitation();
    const hasAttempted = useRef(false);

    useEffect(() => {
        if (hasAttempted.current) return;
        hasAttempted.current = true;

        mutate(token);
    }, [mutate, token]);

    useEffect(() => {
        if (isSuccess && data) {
            navigate({ to: '/projects/$projectId', params: { projectId: data.projectId } });
        }
    }, [isSuccess, data, navigate]);

    return (
        <div className="flex min-h-screen items-center justify-center p-4">
            <div className="w-full max-w-sm rounded-medium border border-white-accent-dark/10 bg-black-accent-default p-6 text-center">
                {isPending && (
                    <>
                        <div className="mx-auto mb-4 h-8 w-8 animate-spin rounded-full border-2 border-primary-default border-t-transparent" />
                        <p className="text-sm text-white-accent-dark">Vérification de l'invitation...</p>
                    </>
                )}
                {isError && (
                    <Alert variant="error" title="Invitation invalide">
                        {error instanceof Error ? error.message : 'Impossible de rejoindre ce projet.'}
                    </Alert>
                )}
            </div>
        </div>
    );
};