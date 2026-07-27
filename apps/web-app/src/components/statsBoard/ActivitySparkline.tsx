import type { Project } from '@/api/projects';

const ACTIVITY_DAYS = 14;

export interface ActivitySparklineProps {
    projects?: Project[];
    isLoading: boolean;
}

export function ActivitySparkline({ projects, isLoading }: ActivitySparklineProps) {
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

            <div
                className="mt-5 flex h-16 items-end gap-1"
                role="img"
                aria-label="Nombre d'espaces créés par jour sur les 14 derniers jours"
            >
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