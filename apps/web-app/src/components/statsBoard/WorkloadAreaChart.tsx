import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { Task } from '@/api/task';

export interface WorkloadAreaChartProps {
    tasks?: Task[];
    isLoading: boolean;
}

interface CustomTooltipProps {
    active?: boolean;
    payload?: Array<{
        value?: number;
        payload?: { label?: string };
    }>;
}

function CustomTooltip({ active, payload }: CustomTooltipProps) {
    if (!active || !payload?.length) return null;
    const point = payload[0];
    const value = point.value ?? 0;

    return (
        <div className="rounded-small border border-white-accent-dark/15 bg-black-accent-dark px-3 py-2 shadow-lg">
            <span className="block text-[10px] uppercase tracking-wider text-white-accent-dark">
                Semaine du {point.payload?.label}
            </span>
            <span className="font-mono text-sm font-semibold text-white-accent-light">
                {value} pt{value > 1 ? 's' : ''} de charge
            </span>
        </div>
    );
}

export function WorkloadAreaChart({ tasks, isLoading }: WorkloadAreaChartProps) {
    const list = tasks ?? [];

    const getMondays = () => {
        const now = new Date();
        const day = now.getDay() || 7;
        const currentMonday = new Date(now);
        currentMonday.setDate(now.getDate() - day + 1);
        currentMonday.setHours(0, 0, 0, 0);

        return Array.from({ length: 5 }).map((_, i) => {
            const d = new Date(currentMonday);
            d.setDate(d.getDate() + (i * 7));
            return d;
        });
    };

    const toYMD = (d: Date | string) => {
        const date = new Date(d);
        return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    };

    const mondays = getMondays();

    const data = mondays.map((monday) => {
        const targetYMD = toYMD(monday);

        const points = list.reduce((sum, task) => {
            if (!task.isChecked && task.targetWeek && toYMD(task.targetWeek) === targetYMD) {
                return sum + (task.storyPoints || 0);
            }
            return sum;
        }, 0);

        return {
            label: monday.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }),
            tick: monday.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }),
            points: points,
        };
    });

    const totalFuturePoints = data.reduce((sum, d) => sum + d.points, 0);
    const hasData = totalFuturePoints > 0;

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default p-5">
            <div className="flex items-baseline justify-between">
                <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                    Plan de charge (5 semaines)
                </span>
                {!isLoading && (
                    <span className="font-mono text-xs text-white-accent-dark">
                        Total à venir : <strong className="text-white-accent-light">{totalFuturePoints} pts</strong>
                    </span>
                )}
            </div>

            <div className="mt-4 h-48">
                {isLoading ? (
                    <div className="h-full w-full animate-pulse rounded bg-black-accent-light" />
                ) : !hasData ? (
                    <div className="flex h-full items-center justify-center rounded-small border border-dashed border-white-accent-dark/15">
                        <span className="text-xs italic text-white-accent-dark">
                            Aucune tâche planifiée
                        </span>
                    </div>
                ) : (
                    <ResponsiveContainer width="100%" height="100%">
                        <AreaChart data={data} margin={{ top: 4, right: 4, left: -20, bottom: 0 }}>
                            <defs>
                                <linearGradient id="pointsFill" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="0%" stopColor="var(--color-primary-default)" stopOpacity={0.4} />
                                    <stop offset="100%" stopColor="var(--color-primary-default)" stopOpacity={0} />
                                </linearGradient>
                            </defs>
                            <CartesianGrid strokeDasharray="3 3" stroke="var(--color-white-dark)" strokeOpacity={0.08} vertical={false} />
                            <XAxis dataKey="tick" tick={{ fill: 'var(--color-white-dark)', fontSize: 10 }} axisLine={{ stroke: 'var(--color-white-dark)', strokeOpacity: 0.15 }} tickLine={false} />
                            <YAxis allowDecimals={false} tick={{ fill: 'var(--color-white-dark)', fontSize: 10 }} axisLine={false} tickLine={false} width={24} />
                            <Tooltip content={<CustomTooltip />} cursor={{ stroke: 'var(--color-white-dark)', strokeOpacity: 0.2 }} />
                            <Area type="monotone" dataKey="points" stroke="var(--color-primary-default)" strokeWidth={2} fill="url(#pointsFill)" />
                        </AreaChart>
                    </ResponsiveContainer>
                )}
            </div>
        </div>
    );
}