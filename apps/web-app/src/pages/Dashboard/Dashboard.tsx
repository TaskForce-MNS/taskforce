import { useSuspenseQuery } from '@tanstack/react-query';
import { Button } from '@/components/atoms/Button';
import { dashboardQueryOptions } from '@/api/queries/dashboardQueries';

export const Dashboard = () => {
    const { data } = useSuspenseQuery(dashboardQueryOptions);

    return (
        <div className="space-y-xl animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* En-tête de la page */}
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                    <h1 className="font-title text-3xl font-bold text-white-accent-light">
                        Mon Tableau de Bord
                    </h1>
                    <p className="mt-1 font-text text-sm text-white-accent-dark">
                        Voici un résumé de tes activités récentes.
                    </p>
                </div>
                <Button variant="primary" size="md">
                    + Nouvelle Tâche
                </Button>
            </div>

            {/* Grille de contenu */}
            {data?.tasks && data.tasks.length > 0 ? (
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {data.tasks.map((task, index) => (
                        <div key={index} className="rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-6 shadow-lg transition-transform hover:-translate-y-1">
                            <h3 className="font-semibold text-white-accent-light">Tâche #{index + 1}</h3>
                            <pre className="mt-2 text-xs text-white-accent-dark overflow-hidden">
                                {JSON.stringify(task, null, 2)}
                            </pre>
                        </div>
                    ))}
                </div>
            ) : (
                // État vide (Empty State)
                <div className="flex flex-col items-center justify-center rounded-large border border-dashed border-white-accent-dark/30 bg-black-accent-dark/50 py-24 text-center">
                    <span className="text-4xl mb-4">🚀</span>
                    <h3 className="mb-2 font-title text-xl font-semibold text-white-accent-light">
                        Aucune tâche pour le moment
                    </h3>
                    <p className="max-w-sm font-text text-sm text-white-accent-dark">
                        Dès que ton backend C# sera branché et que tu auras créé des tâches, elles apparaîtront ici.
                    </p>
                </div>
            )}
        </div>
    );
};