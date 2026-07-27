import type { Project } from '@/api/projects';

export interface ProjectDistributionProps {
    projects?: Project[];
    isLoading: boolean;
}

export function ProjectDistribution({ projects, isLoading }: ProjectDistributionProps) {
    const list = projects ?? [];
    const total = list.length;
    const radius = 15.9155; // circonférence = 100, simplifie le calcul des %
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
                    <svg
                        viewBox="0 0 36 36"
                        className="h-24 w-24 shrink-0 -rotate-90"
                        role="img"
                        aria-label={`${total} espaces répartis par couleur`}
                    >
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
                            style={{
                                transform: 'rotate(90deg)',
                                transformOrigin: '18px 18px',
                                fill: 'var(--color-white-light)',
                                fontSize: '8px',
                                fontFamily: 'monospace',
                                fontWeight: 600,
                            }}
                        >
                            {total}
                        </text>
                    </svg>
                )}

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