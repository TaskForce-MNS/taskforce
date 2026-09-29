import { useState, useRef, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { projectMembersQueryOptions } from '@/api/queries/projectsQueries';
import { useUpdateTask } from '@/mutations/task';
import { type Task } from '@/api/task';

interface AssigneeDropdownProps {
    task: Task;
    projectId: string;
}

export const AssigneeDropdown = ({ task, projectId }: AssigneeDropdownProps) => {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const { data: members, isLoading } = useQuery(projectMembersQueryOptions(projectId));
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

    const handleSelect = (userId: string | null) => {
        if (task.assigneeId === userId) {
            setIsOpen(false);
            return;
        } 
        const payload = userId
            ? { assigneeId: userId }
            : { unassignTask: true };

        updateTask({
            taskId: task.id,
            payload: payload
        });
        setIsOpen(false);
    };

    const isWorking = isLoading || isPending;

    return (
        <div className="relative" ref={dropdownRef}>
            <button
                onClick={() => setIsOpen(!isOpen)}
                disabled={isWorking || task.isChecked}
                className="flex items-center gap-1.5 bg-[#333333] hover:bg-[#404040] text-gray-200 px-2 py-1 rounded-md text-xs transition-colors disabled:opacity-50"
            >
                {isWorking || task.isChecked ? (
                    <span>{task.assigneeName}</span>
                ) : task.assigneeName ? (
                    <>
                        <span className="text-gray-400">Assigner à :</span>
                        <span>{task.assigneeName}</span>
                    </>
                ) : (
                    <span className="text-gray-400 border border-dashed border-gray-500 px-1 rounded">
                        Assigné à personne
                    </span>
                )}
            </button>

            {isOpen && (
                <div className="absolute top-full mt-1 left-0 w-48 bg-[#2A2A2A] border border-[#404040] rounded-md shadow-lg py-1 z-10">
                    <div className="px-3 py-1.5 border-b border-[#404040] text-xs font-semibold text-gray-400">
                        Assigner à...
                    </div>

                    <ul className="max-h-48 overflow-y-auto">
                        {task.assigneeId && (
                            <li>
                                <button
                                    onClick={() => handleSelect(null)}
                                    className="w-full text-left px-3 py-2 text-sm text-red-400 hover:bg-[#333333] flex items-center gap-2 transition-colors"
                                >
                                    X Retirer l'assignation
                                </button>
                            </li>
                        )}

                        {members?.map((member) => (
                            <li key={member.userId}>
                                <button
                                    onClick={() => handleSelect(member.userId)}
                                    className={`w-full text-left px-3 py-2 text-sm flex items-center justify-between hover:bg-[#333333] transition-colors ${task.assigneeId === member.userId ? 'text-blue-400 bg-[#333333]' : 'text-gray-200'
                                        }`}
                                >
                                    <span className="truncate">
                                        {member.firstName} {member.lastName}
                                    </span>
                                    <span className="text-[10px] text-gray-500 px-1.5 py-0.5 rounded-full border border-gray-600">
                                        {member.role}
                                    </span>
                                </button>
                            </li>
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
};