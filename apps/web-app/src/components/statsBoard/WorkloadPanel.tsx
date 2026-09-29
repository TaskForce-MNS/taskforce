import { Link } from '@tanstack/react-router';
import type { Project } from '@/api/project';

const MAX_POINTS_PER_WEEK = 20;
const TOTAL_TICKS = 40;

const getCapacityState = (points: number) => {
    if (points >= 18) return { label: 'Surcharge critique', dot: 'bg-red-500', text: 'text-red-500', fill: 'bg-red-500' };
    if (points >= 14) return { label: 'Rythme soutenu', dot: 'bg-orange-400', text: 'text-orange-400', fill: 'bg-orange-400' };
    return { label: 'Capacité optimale', dot: 'bg-green-400', text: 'text-green-400', fill: 'bg-green-400' };
};

export interface WorkloadPanelProps {
    workloadPoints: number;
    isUserLoading: boolean;
    activeProjectsCount: number;
    latestProject: Project | null;
    isProjectsLoading: boolean;
    upcomingDeadlinesCount?: number;
}

export function WorkloadPanel({
    workloadPoints = 0,
    isUserLoading,
    activeProjectsCount,
    latestProject,
    isProjectsLoading,
    upcomingDeadlinesCount = 0,
}: WorkloadPanelProps) {
    const state = getCapacityState(workloadPoints);

    // Calcul du pourcentage (arrondi à l'entier pour un affichage propre)
    const fillPercentage = Math.round(Math.min((workloadPoints / MAX_POINTS_PER_WEEK) * 100, 100));
    const filledTicks = Math.round((fillPercentage / 100) * TOTAL_TICKS);

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default">
            <div className="p-5">
                <div className="flex items-center justify-between">
                    <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                        Charge de la semaine
                    </span>
                    <span className="flex items-center gap-1.5 font-text text-xs font-medium">
                        <span className={`h-1.5 w-1.5 rounded-full ${state.dot}`} aria-hidden="true" />
                        <span className={state.text}>{state.label}</span>
                    </span>
                </div>

                <div className="mt-3 flex items-end gap-4">
                    {isUserLoading ? (
                        <div className="h-9 w-24 animate-pulse rounded bg-black-accent-light" />
                    ) : (
                        <div className="flex items-baseline gap-2">
                            <span className="font-mono text-3xl font-semibold tabular-nums text-white-accent-light">
                                {fillPercentage}%
                            </span>
                            <span className="font-text text-xs font-medium text-white-accent-dark/60">
                                ({workloadPoints}/{MAX_POINTS_PER_WEEK} pts)
                            </span>
                        </div>
                    )}

                    <div
                        role="progressbar"
                        aria-valuenow={fillPercentage}
                        aria-valuemin={0}
                        aria-valuemax={100}
                        aria-label="Charge de travail actuelle en pourcentage"
                        className="flex flex-1 items-center gap-[3px] pb-1.5"
                    >
                        {Array.from({ length: TOTAL_TICKS }).map((_, i) => (
                            <span
                                key={i}
                                aria-hidden="true"
                                className={`h-4 flex-1 rounded-[1px] transition-colors duration-300
                                    ${i < filledTicks ? state.fill : 'bg-black-accent-light'}`}
                            />
                        ))}
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 border-t border-white-accent-dark/10 md:grid-cols-3 [&>*:not(:last-child)]:border-b [&>*:not(:last-child)]:border-white-accent-dark/10 md:[&>*:not(:last-child)]:border-b-0 md:[&>*:not(:last-child)]:border-r">
                <div className="p-4">
                    <span className="font-text text-[11px] font-semibold uppercase tracking-wider text-white-accent-dark">
                        Espaces actifs
                    </span>
                    <div className="mt-1.5">
                        {isProjectsLoading ? (
                            <div className="h-6 w-8 animate-pulse rounded bg-black-accent-light" />
                        ) : (
                            <span className="font-mono text-xl font-semibold tabular-nums text-white-accent-light">
                                {String(activeProjectsCount).padStart(2, '0')}
                            </span>
                        )}
                    </div>
                </div>

                <div className="p-4">
                    <span className="font-text text-[11px] font-semibold uppercase tracking-wider text-white-accent-dark">
                        Dernière activité
                    </span>
                    <div className="mt-1.5">
                        {latestProject ? (
                            <Link
                                to="/projects/$projectId"
                                params={{ projectId: latestProject.id }}
                                className="flex items-center gap-1.5 text-sm font-medium text-white-accent-light transition-colors hover:text-primary-default"
                            >
                                <span className="truncate">{latestProject.name}</span>
                                <span aria-hidden="true" className="shrink-0 text-white-accent-dark">→</span>
                            </Link>
                        ) : (
                            <span className="text-sm italic text-white-accent-dark">Aucune activité</span>
                        )}
                    </div>
                </div>

                <div className="p-4">
                    <div className="flex items-center justify-between">
                        <span className="font-text text-[11px] font-semibold uppercase tracking-wider text-white-accent-dark">
                            Tâches en retard
                        </span>
                        <span className="rounded-full bg-black-accent-light px-1.5 py-0.5 font-text text-[9px] font-medium text-white-accent-dark">
                            Urgent
                        </span>
                    </div>
                    <div className="mt-1.5">
                        {isUserLoading ? (
                            <div className="h-6 w-8 animate-pulse rounded bg-black-accent-light" />
                        ) : (
                            <span className={`font-mono text-xl font-semibold tabular-nums ${upcomingDeadlinesCount > 0 ? 'text-red-400' : 'text-white-accent-dark/40'}`}>
                                {upcomingDeadlinesCount > 0 ? upcomingDeadlinesCount : '—'}
                            </span>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}