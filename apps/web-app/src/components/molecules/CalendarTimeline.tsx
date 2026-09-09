import { useState, useEffect, useRef, useCallback, forwardRef, useImperativeHandle } from 'react';

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
        const scrollTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

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
            if (scrollTimeoutRef.current) clearTimeout(scrollTimeoutRef.current);
            scrollTimeoutRef.current = setTimeout(() => {
                if (projectId && projectId !== 'undefined') {
                    sessionStorage.setItem(`calendar-scroll-${projectId}`, container.scrollTop.toString());
                }
            }, 150);

            const dividers = container.querySelectorAll('.date-divider');
            const containerTop = container.getBoundingClientRect().top;

            let activeDateTitle = null;

            dividers.forEach((divider) => {
                const htmlElement = divider as HTMLElement;
                const distanceToTop = htmlElement.getBoundingClientRect().top - containerTop;

                if (distanceToTop <= 120) {
                    activeDateTitle = htmlElement.getAttribute('data-full-date');
                }

                htmlElement.style.opacity = distanceToTop < 40 ? '0' : '1';
            });

            if (activeDateTitle) {
                onDateChange(activeDateTitle);
            }
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
        }, [projectId, handleScroll]);

        return (
            <div
                ref={containerRef}
                onScroll={handleScroll}
                className="flex-1 min-h-0 w-full overflow-y-auto rounded-xl border border-white-accent-dark/15 bg-black-accent-light/10 p-4 sm:p-6 shadow-inner scrollbar-hide relative [mask-image:linear-gradient(to_bottom,transparent,black_20px,black_calc(100%-20px),transparent)]"
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
                                    className="date-divider flex items-center gap-4 py-2 -mx-4 px-4 sm:-mx-6 sm:px-6 transition-opacity duration-200"
                                    data-full-date={fullDate}
                                >
                                    <span className={`text-sm font-semibold shrink-0 ${isToday ? 'text-primary-light' : 'text-white-accent-light'}`}>
                                        {isToday ? "Aujourd'hui" : shortDate}
                                    </span>
                                    <div className={`h-[2px] flex-1 rounded-full ${isToday ? 'bg-primary-default/50' : 'bg-white-accent-dark/20'}`}></div>
                                </div>

                                <div className="mt-4 flex flex-col gap-3 pl-2 sm:pl-4 border-l-2 border-white-accent-dark/10 ml-[22px]">
                                    <div className="h-20 w-3/4 rounded-lg bg-black-accent-light/30 border border-white-accent-dark/10 flex items-center px-4">
                                        <p className="text-white-accent-dark text-xs italic">Tâche fictive...</p>
                                    </div>
                                    <div className="h-16 w-1/2 rounded-lg bg-black-accent-light/30 border border-white-accent-dark/10"></div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>
        );
    });
