import { Radar, RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, ResponsiveContainer, Tooltip } from 'recharts';
import type { Task } from '@/api/task';

export interface TaskDifficultyRadarProps {
    tasks?: Task[];
    isLoading: boolean;
}

function CustomTooltip({
    active,
    payload,
}: {
    active?: boolean;
    payload?: Array<{ value?: number; payload: { category: string } }>;
}) {
    if (!active || !payload?.length) return null;
    const point = payload[0];

    return (
        <div className="rounded-small border border-white-accent-dark/15 bg-black-accent-dark px-3 py-2 shadow-lg">
            <span className="block text-[10px] uppercase tracking-wider text-white-accent-dark">
                Tâches {point.payload.category}s
            </span>
            <span className="font-mono text-sm font-semibold text-white-accent-light">
                {point.value ?? 0} pt{(point.value ?? 0) > 1 ? 's' : ''} au total
            </span>
        </div>
    );
}

export function TaskDifficultyRadar({ tasks, isLoading }: TaskDifficultyRadarProps) {
    const list = tasks ?? [];

    let simple = 0, medium = 0, complex = 0;

    list.filter(t => !t.isChecked).forEach(t => {
        if (t.difficulty === 1) simple += t.storyPoints;
        else if (t.difficulty === 2) medium += t.storyPoints;
        else if (t.difficulty === 3) complex += t.storyPoints;
    });

    const data = [
        { category: 'Simple', points: simple },
        { category: 'Moyenne', points: medium },
        { category: 'Complexe', points: complex }
    ];

    const totalPoints = simple + medium + complex;
    const hasData = totalPoints > 0;

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default p-5">
            <div className="flex items-baseline justify-between">
                <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                    Profil d'effort
                </span>
            </div>

            <div className="mt-2 h-48">
                {isLoading ? (
                    <div className="h-full w-full animate-pulse rounded bg-black-accent-light" />
                ) : !hasData ? (
                    <div className="flex h-full items-center justify-center rounded-small border border-dashed border-white-accent-dark/15">
                        <span className="text-xs italic text-white-accent-dark">
                            Aucune tâche en cours
                        </span>
                    </div>
                ) : (
                    <ResponsiveContainer width="100%" height="100%">
                        <RadarChart data={data} margin={{ top: 8, right: 20, bottom: 8, left: 20 }}>
                            <PolarGrid stroke="var(--color-white-dark)" strokeOpacity={0.12} />
                            <PolarAngleAxis dataKey="category" tick={{ fill: 'var(--color-white-dark)', fontSize: 11 }} />
                            <PolarRadiusAxis allowDecimals={false} tick={{ fill: 'var(--color-white-dark)', fontSize: 9 }} axisLine={false} />
                            <Tooltip content={<CustomTooltip />} />
                            <Radar dataKey="points" stroke="var(--color-secondary-default)" strokeWidth={2} fill="var(--color-secondary-default)" fillOpacity={0.3} />
                        </RadarChart>
                    </ResponsiveContainer>
                )}
            </div>
            <p className="mt-1 text-center text-[10px] text-white-accent-dark">
                Basé sur les story points des tâches à faire
            </p>
        </div>
    );
}