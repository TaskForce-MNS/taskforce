import { useQuery } from '@tanstack/react-query';
import { projectTasksQueryOptions } from '@/api/queries/taskQueries';
import { TaskItem } from './TaskItem';

interface TaskListProps {
    projectId: string;
    targetDate?: Date;
}

export const TaskList = ({ projectId, targetDate }: TaskListProps) => {
    const { data: tasks, isLoading, isError } = useQuery(projectTasksQueryOptions(projectId));

    if (isLoading) return <div className="text-gray-400 p-4">Chargement des tâches...</div>;
    if (isError) return <div className="text-red-400 p-4">Erreur lors du chargement des tâches.</div>;

    const visibleTasks = tasks?.filter(task => {
        if (task.isArchived) return false;

        if (!targetDate) return true;

        const taskDateValue = task.dueDate ? task.dueDate : task.createdAt;
        const taskDate = new Date(taskDateValue);

        return taskDate.toDateString() === targetDate.toDateString();
    }) || [];

    if (targetDate && visibleTasks.length === 0) {
        return null;
    }

    return (
        <div className={`flex flex-col gap-4 ${targetDate ? 'p-0' : 'p-6 min-h-screen'}`}>
            {!targetDate && (
                <div className="flex items-center justify-between mb-2">
                    <span className="text-gray-500 text-sm">{visibleTasks.length} tâches</span>
                </div>
            )}

            {visibleTasks.length === 0 ? (
                <div className="text-gray-500 italic">Aucune tâche en cours.</div>
            ) : (
                <div className="flex flex-col gap-3">
                    {visibleTasks.map(task => (
                        <TaskItem key={task.id} task={task} projectId={projectId} />
                    ))}
                </div>
            )}
        </div>
    );
};