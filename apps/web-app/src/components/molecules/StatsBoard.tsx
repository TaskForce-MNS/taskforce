import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/useAuthStore';
import { projectsQueryOptions } from '@/api/queries/projectsQueries';
import { Link } from '@tanstack/react-router';
import type { Project } from '@/api/projects';

const TOTAL_TICKS = 40;
const ACTIVITY_DAYS = 14;

const getCapacityState = (val: number) => {
    if (val >= 90) return { label: 'Surcharge critique', dot: 'bg-error', text: 'text-error', fill: 'bg-error' };
    if (val >= 75) return { label: 'Rythme soutenu', dot: 'bg-warning', text: 'text-warning', fill: 'bg-warning' };
    return { label: 'Capacité optimale', dot: 'bg-success', text: 'text-success', fill: 'bg-success' };
};

// ────────────────────────────────────────────────────────────
// Panneau principal : charge de travail (inchangé)
// ────────────────────────────────────────────────────────────
export const StatsBoard = () => {
    const user = useAuthStore((state) => state.user);
    const isUserLoading = useAuthStore((state) => state.isLoading);
    const workload = user?.workload ?? 0;

    const { data: projects, isLoading: isProjectsLoading } = useQuery(projectsQueryOptions);
    const activeProjectsCount = projects?.length ?? 0;

    const latestProject = projects && projects.length > 0
        ? [...projects].sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())[0]
        : null;

    const state = getCapacityState(workload);
    const filledTicks = Math.round((Math.min(workload, 100) / 100) * TOTAL_TICKS);

    return (
        <div className="flex flex-col gap-4">

            {/* ── Panneau de charge ── */}
            <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default">
                <div className="p-5">
                    <div className="flex items-center justify-between">
                        <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                            Charge actuelle
                        </span>
                        <span className="flex items-center gap-1.5 font-text text-xs font-medium">
                            <span className={`h-1.5 w-1.5 rounded-full ${state.dot}`} aria-hidden="true" />
                            <span className={state.text}>{state.label}</span>
                        </span>
                    </div>

                    <div className="mt-3 flex items-end gap-4">
                        {isUserLoading ? (
                            <div className="h-9 w-16 animate-pulse rounded bg-black-accent-light" />
                        ) : (
                            <span className="font-mono text-3xl font-semibold tabular-nums text-white-accent-light">
                                {workload}%
                            </span>
                        )}

                        <div
                            role="progressbar"
                            aria-valuenow={workload}
                            aria-valuemin={0}
                            aria-valuemax={100}
                            aria-label="Charge de travail actuelle"
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
                                Échéances
                            </span>
                            <span className="rounded-full bg-black-accent-light px-1.5 py-0.5 font-text text-[9px] font-medium text-white-accent-dark">
                                À venir
                            </span>
                        </div>
                        <div className="mt-1.5">
                            <span className="font-mono text-xl font-semibold text-white-accent-dark/40">—</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* ── Ligne d'insights visuels ── */}
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <ProjectDistribution projects={projects} isLoading={isProjectsLoading} />
                <ActivitySparkline projects={projects} isLoading={isProjectsLoading} />
            </div>
        </div>
    );
};

// ────────────────────────────────────────────────────────────
// Donut : répartition des projets par couleur
// ────────────────────────────────────────────────────────────
function ProjectDistribution({ projects, isLoading }: { projects?: Project[]; isLoading: boolean }) {
    const list = projects ?? [];
    const total = list.length;
    const radius = 15.9155; // rayon donnant une circonférence de 100 (facilite les % en stroke-dasharray)
    const circumference = 2 * Math.PI * radius;

    let cursor = 0;
    const segments = list.map((project) => {
        const fraction = total > 0 ? 1 / total : 0;
        const dash = fraction * circumference;
        const segment = {
            id: project.id,
            name: project.name,
            color: project.colorHex ?? '#587B7F',
            dashArray: `${dash} ${circumference - dash}`,
            dashOffset: -cursor,
        };
        cursor += dash;
        return segment;
    });

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default p-5">
            <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                Répartition des espaces
            </span>

            <div className="mt-4 flex items-center gap-5">
                {isLoading ? (
                    <div className="h-24 w-24 shrink-0 animate-pulse rounded-full bg-black-accent-light" />
                ) : total === 0 ? (
                    <div className="flex h-24 w-24 shrink-0 items-center justify-center rounded-full border-4 border-dashed border-white-accent-dark/15">
                        <span className="text-[10px] text-white-accent-dark">Aucun</span>
                    </div>
                ) : (
                    <svg viewBox="0 0 36 36" className="h-24 w-24 shrink-0 -rotate-90" role="img" aria-label={`${total} espaces répartis par couleur`}>
                        <circle cx="18" cy="18" r={radius} fill="none" stroke="var(--color-black-light)" strokeWidth="4" />
                        {segments.map((seg) => (
                            <circle
                                key={seg.id}
                                cx="18" cy="18" r={radius}
                                fill="none"
                                stroke={seg.color}
                                strokeWidth="4"
                                strokeDasharray={seg.dashArray}
                                strokeDashoffset={seg.dashOffset}
                                strokeLinecap="butt"
                            />
                        ))}
                        <text
                            x="18" y="18"
                            textAnchor="middle"
                            dominantBaseline="central"
                            className="rotate-90"
                            style={{ transform: 'rotate(90deg)', transformOrigin: '18px 18px', fill: 'var(--color-white-light)', fontSize: '8px', fontFamily: 'monospace', fontWeight: 600 }}
                        >
                            {total}
                        </text>
                    </svg>
                )}

                {/* Légende */}
                <ul className="flex min-w-0 flex-1 flex-col gap-1.5">
                    {isLoading ? (
                        <>
                            <div className="h-4 w-full animate-pulse rounded bg-black-accent-light" />
                            <div className="h-4 w-2/3 animate-pulse rounded bg-black-accent-light" />
                        </>
                    ) : total === 0 ? (
                        <li className="text-xs italic text-white-accent-dark">Créez votre premier espace</li>
                    ) : (
                        list.map((project) => (
                            <li key={project.id} className="flex items-center gap-2 truncate text-xs text-white-accent-light">
                                <span
                                    className="h-2 w-2 shrink-0 rounded-full"
                                    style={{ backgroundColor: project.colorHex ?? '#587B7F' }}
                                    aria-hidden="true"
                                />
                                <span className="truncate">{project.name}</span>
                            </li>
                        ))
                    )}
                </ul>
            </div>
        </div>
    );
}

// ────────────────────────────────────────────────────────────
// Frise : activité de création sur les 14 derniers jours
// ────────────────────────────────────────────────────────────
function ActivitySparkline({ projects, isLoading }: { projects?: Project[]; isLoading: boolean }) {
    const days = Array.from({ length: ACTIVITY_DAYS }).map((_, i) => {
        const date = new Date();
        date.setDate(date.getDate() - (ACTIVITY_DAYS - 1 - i));
        date.setHours(0, 0, 0, 0);
        return date;
    });

    const counts = days.map((day) => {
        const next = new Date(day);
        next.setDate(next.getDate() + 1);
        return (projects ?? []).filter((p) => {
            const created = new Date(p.createdAt);
            return created >= day && created < next;
        }).length;
    });

    const max = Math.max(1, ...counts);
    const hasActivity = counts.some((c) => c > 0);

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default p-5">
            <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                Activité de création · 14 derniers jours
            </span>

            <div className="mt-5 flex h-16 items-end gap-1" role="img" aria-label="Nombre d'espaces créés par jour sur les 14 derniers jours">
                {isLoading ? (
                    <div className="h-full w-full animate-pulse rounded bg-black-accent-light" />
                ) : (
                    counts.map((count, i) => {
                        const heightPct = Math.max(8, (count / max) * 100);
                        return (
                            <div
                                key={i}
                                className={`flex-1 rounded-[1px] transition-all duration-300 ${count > 0 ? 'bg-primary-default' : 'bg-black-accent-light'}`}
                                style={{ height: `${heightPct}%` }}
                                title={`${count} créé(s) le ${days[i].toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' })}`}
                            />
                        );
                    })
                )}
            </div>

            <div className="mt-2 flex items-center justify-between text-[10px] text-white-accent-dark">
                <span>il y a {ACTIVITY_DAYS} jours</span>
                {!isLoading && !hasActivity && <span className="italic">Pas d'activité récente</span>}
                <span>aujourd'hui</span>
            </div>
        </div>
    );
}