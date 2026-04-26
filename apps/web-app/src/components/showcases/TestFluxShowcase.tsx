import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/atoms/Button';
import { Alert } from '@/components/atoms/Alert';

export const TestFluxShowcase = () => {
    // TanStack Query gère tous les états pour toi !
    const { data, error, isFetching, refetch, isSuccess, isError } = useQuery({
        queryKey: ['test-db-connection'],
        queryFn: async () => {
            const response = await fetch('https://api.taskforce.local/api/debug/test-db');
            if (!response.ok) throw new Error(`Erreur réseau : ${response.status}`);
            return response.json();
        },
        enabled: false, // On bloque l'exécution automatique au chargement de la page
    });

    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                🔧 Test du Flux Docker (CORS & DB)
            </h2>

            <div className="space-y-6">
                <Button
                    onClick={() => refetch()}
                    isLoading={isFetching}
                >
                    Pinger la Base de Données
                </Button>

                {isSuccess && data && (
                    <Alert variant="success" title="Flux validé !">
                        <p>Les données traversent bien React ➔ API ➔ DB ➔ React.</p>
                        <pre className="mt-2 p-2 bg-black-accent-dark rounded text-xs overflow-x-auto">
                            {JSON.stringify(data, null, 2)}
                        </pre>
                    </Alert>
                )}

                {isError && (
                    <Alert variant="error" title="Échec du flux">
                        <p>Impossible de joindre la base de données ou l'API.</p>
                        <p className="font-mono text-xs mt-1 opacity-80">{error.message}</p>
                    </Alert>
                )}
            </div>
        </section>
    );
};