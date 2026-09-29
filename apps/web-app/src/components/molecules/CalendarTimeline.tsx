import { useState, useEffect, useRef, useCallback, forwardRef, useImperativeHandle } from 'react';
import { TaskList } from '@/components/molecules/task/TaskList';

interface CalendarTimelineProps {
    projectId: string;
    onDateChange: (newDateTitle: string) => void;
}
export interface CalendarTimelineHandle {
    scrollToToday: () => void;
}

const generateDummyDays = () => {
    const days = [];
    for (let i = 20; i >= -5; i--) {
        const d = new Date();
        d.setDate(d.getDate() + i);
        days.push(d);
    }
    return days;
};

export const CalendarTimeline = forwardRef<CalendarTimelineHandle, CalendarTimelineProps>(
    ({ projectId, onDateChange }, ref) => {
        const [days] = useState(generateDummyDays());

        const containerRef = useRef<HTMLDivElement>(null);
        const dividersRef = useRef<Map<string, HTMLDivElement>>(new Map());

        const storageTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
        const scrollRafRef = useRef<number | null>(null);

        const lastActiveDateRef = useRef<string | null>(null);

        useImperativeHandle(ref, () => ({
            scrollToToday: () => {
                if (containerRef.current) {
                    const todayElement = containerRef.current.querySelector('[data-is-today="true"]');
                    if (todayElement) {
                        todayElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                }
            }
        }));

        const formatFullDate = (date: Date) => {
            const str = date.toLocaleDateString('fr-FR', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
                year: 'numeric',
            });
            return str.charAt(0).toUpperCase() + str.slice(1);
        };

        const handleScroll = useCallback(() => {
            const container = containerRef.current;
            if (!container) return;

            // 1. Sauvegarde différée (Debounce) : N'écrit dans le stockage que si on a arrêté de scroller pendant 150ms
            if (storageTimeoutRef.current) clearTimeout(storageTimeoutRef.current);
            storageTimeoutRef.current = setTimeout(() => {
                if (projectId && projectId !== 'undefined') {
                    sessionStorage.setItem(`calendar-scroll-${projectId}`, container.scrollTop.toString());
                }
            }, 150);

            // 2. Animation (requestAnimationFrame) : Calcule l'opacité à 60 images/seconde sans bloquer le navigateur
            if (scrollRafRef.current) return; // Empêche l'accumulation de calculs

            scrollRafRef.current = requestAnimationFrame(() => {
                const containerTop = container.getBoundingClientRect().top;
                let activeDateTitle: string | null = null;

                // 🌟 SÉCURITÉ : On boucle sur nos références React, pas sur le vrai DOM
                dividersRef.current.forEach((htmlElement, fullDate) => {
                    const distanceToTop = htmlElement.getBoundingClientRect().top - containerTop;

                    if (distanceToTop <= 120) {
                        activeDateTitle = fullDate;
                    }

                    // Calcul de l'opacité
                    htmlElement.style.opacity = distanceToTop < 40 ? '0' : '1';
                });

                if (activeDateTitle && activeDateTitle !== lastActiveDateRef.current) {
                    lastActiveDateRef.current = activeDateTitle;
                    onDateChange(activeDateTitle);
                }

                scrollRafRef.current = null;
            });
        }, [onDateChange, projectId]);

        useEffect(() => {
            if (!containerRef.current || !projectId || projectId === 'undefined') return;
            const container = containerRef.current;

            const savedScroll = sessionStorage.getItem(`calendar-scroll-${projectId}`);

            if (savedScroll !== null) {
                container.scrollTop = parseInt(savedScroll, 10);
            } else {
                const todayElement = container.querySelector('[data-is-today="true"]');
                if (todayElement) todayElement.scrollIntoView({ block: 'start' });
            }

            handleScroll();
            const layoutTimeout = setTimeout(() => {
                handleScroll();
            }, 300);
            return () => {
                if (scrollRafRef.current) cancelAnimationFrame(scrollRafRef.current);
                clearTimeout(layoutTimeout);
            };
        }, [projectId, handleScroll]);

        return (
            <div
                ref={containerRef}
                onScroll={handleScroll}
                className="flex-1 min-h-0 w-full overflow-y-auto overflow-x-hidden rounded-xl bg-black-accent-light/10 p-4 shadow-inner scrollbar-hide relative [mask-image:linear-gradient(to_bottom,transparent,black_2px,black_calc(100%-20px),transparent)] [mask-image:linear-gradient(to_top,transparent,black_2px,black_calc(100%-10px),transparent)]"
            >
                <div className="flex flex-col gap-12">
                    {days.map((day, index) => {
                        const shortDate = day.toLocaleDateString('fr-FR');
                        const fullDate = formatFullDate(day);
                        const isToday = day.toDateString() === new Date().toDateString();

                        return (
                            <div
                                key={index}
                                className="flex flex-col"
                                data-is-today={isToday}
                            >
                                <div
                                    ref={(el) => {
                                        if (el) dividersRef.current.set(fullDate, el);
                                        else dividersRef.current.delete(fullDate);
                                    }}
                                    className="date-divider flex items-center gap-4 py-2 -mx-4 px-4 sm:-mx-6 sm:px-6 transition-opacity duration-200"
                                >
                                    <span className={`text-sm font-semibold shrink-0 ${isToday ? 'text-primary-light' : 'text-white-accent-light'}`}>
                                        {isToday ? "Aujourd'hui" : shortDate}
                                    </span>
                                    <div className={`h-[2px] flex-1 rounded-full ${isToday ? 'bg-primary-default/50' : 'bg-white-accent-dark/20'}`}></div>
                                </div>

                                <div className="mt-1 flex flex-col">
                                    <TaskList projectId={projectId} targetDate={day} />
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>
        );
    }
);