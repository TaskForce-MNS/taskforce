import { Radar, RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, ResponsiveContainer, Tooltip } from 'recharts';
import type { Project } from '@/api/projects';

const WEEKDAY_LABELS = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];

export interface WeeklyRadarChartProps {
    projects?: Project[];
    isLoading: boolean;
}

interface CustomTooltipProps {
    active?: boolean;
    payload?: Array<{ value: number; payload: { day: string } }>;
}

function CustomTooltip({ active, payload }: CustomTooltipProps) {
    if (!active || !payload?.length) return null;
    const point = payload[0];

    return (
        <div className="rounded-small border border-white-accent-dark/15 bg-black-accent-dark px-3 py-2 shadow-lg">
            <span className="block text-[10px] uppercase tracking-wider text-white-accent-dark">
                {point.payload.day}
            </span>
            <span className="font-mono text-sm font-semibold text-white-accent-light">
                {point.value} création{point.value > 1 ? 's' : ''}
            </span>
        </div>
    );
}

export function WeeklyRadarChart({ projects, isLoading }: WeeklyRadarChartProps) {
    const list = projects ?? [];

    // JS getDay() : 0 = dimanche → on remappe pour commencer à lundi
    const counts = [0, 0, 0, 0, 0, 0, 0];
    list.forEach((p) => {
        const jsDay = new Date(p.createdAt).getDay();
        const index = jsDay === 0 ? 6 : jsDay - 1;
        counts[index] += 1;
    });

    const data = WEEKDAY_LABELS.map((day, i) => ({ day, value: counts[i] }));
    const hasData = list.length > 0;
    const busiestIndex = counts.indexOf(Math.max(...counts));

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default p-5">
            <div className="flex items-baseline justify-between">
                <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                    Répartition hebdomadaire
                </span>
                {!isLoading && hasData && counts[busiestIndex] > 0 && (
                    <span className="font-mono text-xs text-white-accent-dark">
                        pic : <strong className="text-white-accent-light">{WEEKDAY_LABELS[busiestIndex]}</strong>
                    </span>
                )}
            </div>

            <div className="mt-2 h-48">
                {isLoading ? (
                    <div className="h-full w-full animate-pulse rounded bg-black-accent-light" />
                ) : !hasData ? (
                    <div className="flex h-full items-center justify-center rounded-small border border-dashed border-white-accent-dark/15">
                        <span className="text-xs italic text-white-accent-dark">
                            Pas encore assez de données
                        </span>
                    </div>
                ) : (
                    <ResponsiveContainer width="100%" height="100%">
                        <RadarChart data={data} margin={{ top: 8, right: 20, bottom: 8, left: 20 }}>
                            <PolarGrid stroke="var(--color-white-dark)" strokeOpacity={0.12} />
                            <PolarAngleAxis
                                dataKey="day"
                                tick={{ fill: 'var(--color-white-dark)', fontSize: 11 }}
                            />
                            <PolarRadiusAxis
                                allowDecimals={false}
                                tick={{ fill: 'var(--color-white-dark)', fontSize: 9 }}
                                axisLine={false}
                            />
                            <Tooltip content={<CustomTooltip />} />
                            <Radar
                                dataKey="value"
                                stroke="var(--color-secondary-default)"
                                strokeWidth={2}
                                fill="var(--color-secondary-default)"
                                fillOpacity={0.3}
                            />
                        </RadarChart>
                    </ResponsiveContainer>
                )}
            </div>

            <p className="mt-1 text-center text-[10px] text-white-accent-dark">
                Basé sur les dates de création de vos espaces
            </p>
        </div>
    );
}