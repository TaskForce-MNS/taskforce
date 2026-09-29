import { useState, forwardRef, useImperativeHandle } from 'react';

export interface StickyDateTitleHandle {
    setTitle: (title: string) => void;
}

interface StickyDateTitleProps {
    onClick: () => void;
}

const getTodayFormatted = () => {
    const str = new Date().toLocaleDateString('fr-FR', {
        weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
    });
    return str.charAt(0).toUpperCase() + str.slice(1);
};

export const StickyDateTitle = forwardRef<StickyDateTitleHandle, StickyDateTitleProps>(
    ({ onClick }, ref) => {
        const [title, setTitle] = useState(getTodayFormatted());

        useImperativeHandle(ref, () => ({
            setTitle: (newTitle: string) => setTitle(newTitle)
        }));

        return (
            <h1
                onClick={onClick}
                title="Revenir à aujourd'hui"
                className="truncate font-subtitle sm:text-xl font-bold text-white-accent-light transition-colors duration-300 cursor-pointer hover:text-primary-light"
            >
                {title}
            </h1>
        );
    }
);