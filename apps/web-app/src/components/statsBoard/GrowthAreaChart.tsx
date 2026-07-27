import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { Project } from '@/api/projects';

const RANGE_DAYS = 30;

export interface GrowthAreaChartProps {
    projects?: Project[];
    isLoading: boolean;
}

interface CustomTooltipProps {
    active?: boolean;
    payload?: Array<{ value: number; payload: { label: string } }>;
}

function CustomTooltip({ active, payload }: CustomTooltipProps) {
    if (!active || !payload?.length) return null;
    const point = payload[0];

    return (
        <div className="rounded-small border border-white-accent-dark/15 bg-black-accent-dark px-3 py-2 shadow-lg">
            <span className="block text-[10px] uppercase tracking-wider text-white-accent-dark">
                {point.payload.label}
            </span>
            <span className="font-mono text-sm font-semibold text-white-accent-light">
                {point.value} espace{point.value > 1 ? 's' : ''}
            </span>
        </div>
    );
}

export function GrowthAreaChart({ projects, isLoading }: GrowthAreaChartProps) {
    const list = projects ?? [];

    const days = Array.from({ length: RANGE_DAYS }).map((_, i) => {
        const date = new Date();
        date.setDate(date.getDate() - (RANGE_DAYS - 1 - i));
        date.setHours(0, 0, 0, 0);
        return date;
    });

    let running = list.filter((p) => new Date(p.createdAt) < days[0]).length;

    const data = days.map((day) => {
        const next = new Date(day);
        next.setDate(next.getDate() + 1);

        const createdThatDay = list.filter((p) => {
            const created = new Date(p.createdAt);
            return created >= day && created < next;
        }).length;

        running += createdThatDay;

        return {
            label: day.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }),
            tick: day.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }),
            total: running,
        };
    });

    const hasData = list.length > 0;

    return (
        <div className="rounded-small border border-white-accent-dark/10 bg-black-accent-default p-5">
            <div className="flex items-baseline justify-between">
                <span className="font-text text-xs font-semibold uppercase tracking-wider text-white-accent-dark">
                    Croissance des espaces · 30 jours
                </span>
                {!isLoading && (
                    <span className="font-mono text-xs text-white-accent-dark">
                        total : <strong className="text-white-accent-light">{list.length}</strong>
                    </span>
                )}
            </div>

            <div className="mt-4 h-48">
                {isLoading ? (
                    <div className="h-full w-full animate-pulse rounded bg-black-accent-light" />
                ) : !hasData ? (
                    <div className="flex h-full items-center justify-center rounded-small border border-dashed border-white-accent-dark/15">
                        <span className="text-xs italic text-white-accent-dark">
                            Créez un espace pour voir la tendance
                        </span>
                    </div>
                ) : (
                    <ResponsiveContainer width="100%" height="100%">
                        <AreaChart data={data} margin={{ top: 4, right: 4, left: -20, bottom: 0 }}>
                            <defs>
                                <linearGradient id="growthFill" x1="0" y1="0" x2="0" y2="1">
                                    <stop offset="0%" stopColor="var(--color-primary-default)" stopOpacity={0.35} />
                                    <stop offset="100%" stopColor="var(--color-primary-default)" stopOpacity={0} />
                                </linearGradient>
                            </defs>
                            <CartesianGrid
                                strokeDasharray="3 3"
                                stroke="var(--color-white-dark)"
                                strokeOpacity={0.08}
                                vertical={false}
                            />
                            <XAxis
                                dataKey="tick"
                                tick={{ fill: 'var(--color-white-dark)', fontSize: 10 }}
                                axisLine={{ stroke: 'var(--color-white-dark)', strokeOpacity: 0.15 }}
                                tickLine={false}
                                interval={Math.ceil(RANGE_DAYS / 6)}
                            />
                            <YAxis
                                allowDecimals={false}
                                tick={{ fill: 'var(--color-white-dark)', fontSize: 10 }}
                                axisLine={false}
                                tickLine={false}
                                width={24}
                            />
                            <Tooltip content={<CustomTooltip />} cursor={{ stroke: 'var(--color-white-dark)', strokeOpacity: 0.2 }} />
                            <Area
                                type="stepAfter"
                                dataKey="total"
                                stroke="var(--color-primary-default)"
                                strokeWidth={2}
                                fill="url(#growthFill)"
                            />
                        </AreaChart>
                    </ResponsiveContainer>
                )}
            </div>
        </div>
    );
}