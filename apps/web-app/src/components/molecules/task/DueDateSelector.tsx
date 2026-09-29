import { useState, useRef, useEffect, type ChangeEvent } from 'react';
import { useUpdateTask } from '@/mutations/task';
import { type Task } from '@/api/task';

interface DueDateSelectorProps {
    task: Task;
    projectId: string;
}

export const DueDateSelector = ({ task, projectId }: DueDateSelectorProps) => {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);
    const { mutate: updateTask, isPending } = useUpdateTask(projectId);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleDateChange = (e: ChangeEvent<HTMLInputElement>) => {
        const dateValue = e.target.value;
        if (!dateValue) return;

        // On convertit la date locale en format ISO pour le backend C#
        const isoDate = new Date(dateValue).toISOString();
        updateTask({
            taskId: task.id,
            payload: { dueDate: isoDate }
        });
        setIsOpen(false);
    };

    const handleRemoveDate = () => {
        updateTask({
            taskId: task.id,
            payload: { dueDate: null } // null retire la date en base de données
        });
        setIsOpen(false);
    };

    // Formatage de la date pour l'affichage (ex: "06 février 2026")
    const displayDate = task.dueDate
        ? new Intl.DateTimeFormat('fr-FR', { day: '2-digit', month: 'long', year: 'numeric' }).format(new Date(task.dueDate))
        : null;

    return (
        <div className="relative flex items-center" ref={dropdownRef}>
            <button
                onClick={() => setIsOpen(!isOpen)}
                disabled={isPending}
                className="flex items-center gap-2 text-gray-400 hover:text-gray-200 transition-colors text-xs disabled:opacity-50"
            >
                <span>{displayDate || "Ajouter une échéance"}</span>
            </button>

            {/* LE MENU DÉROULANT */}
            {isOpen && (
                <div className="absolute top-full right-0 mt-2 p-2 bg-[#2A2A2A] border border-[#404040] rounded-md shadow-lg z-10 w-48 flex flex-col gap-2">
                    <input
                        type="date"
                        // Pré-remplit l'input si une date existe (format YYYY-MM-DD requis par l'input html)
                        defaultValue={task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : ''}
                        onChange={handleDateChange}
                        className="w-full bg-[#1A1A1A] text-gray-200 text-sm border border-[#404040] rounded p-1.5 focus:outline-none focus:border-blue-500 [color-scheme:dark]"
                    />

                    {task.dueDate && (
                        <button
                            onClick={handleRemoveDate}
                            className="w-full text-left px-2 py-1.5 text-xs text-red-400 hover:bg-[#333333] rounded flex items-center gap-2 transition-colors"
                        >
                            Retirer la date
                        </button>
                    )}
                </div>
            )}
        </div>
    );
};