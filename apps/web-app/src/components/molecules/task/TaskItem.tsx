import { type Task, TaskDifficulty } from '@/api/task';
import { useUpdateTask } from '@/mutations/task';
import { AssigneeDropdown } from './AssigneeDropdown';
import { DueDateSelector } from './DueDateSelector';

interface TaskItemProps {
    task: Task;
    projectId: string;
}

export const TaskItem = ({ task, projectId }: TaskItemProps) => {
    const { mutate: updateTask, isPending } = useUpdateTask(projectId);

    const handleToggle = () => {
        if (isPending) return;
        updateTask({
            taskId: task.id,
            payload: { isChecked: !task.isChecked },
        });
    };

    const getDifficultyLabel = (diff: TaskDifficulty) => {
        switch (diff) {
            case TaskDifficulty.Simple: return { label: 'Simple', color: 'bg-green-500/10 text-green-400 border-green-500/20' };
            case TaskDifficulty.Medium: return { label: 'Moyenne', color: 'bg-orange-500/10 text-orange-400 border-orange-500/20' };
            case TaskDifficulty.Complex: return { label: 'Complexe', color: 'bg-red-500/10 text-red-400 border-red-500/20' };
            default: return null;
        }
    };
    const difficultyInfo = getDifficultyLabel(task.difficulty);

    let daysLeftTag = null;
    if (task.dueDate && !task.isChecked) {
        const diffTime = new Date(task.dueDate).getTime() - new Date().getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays < 0) {
            daysLeftTag = <span className="text-red-400">En retard de {Math.abs(diffDays)}j</span>;
        } else if (diffDays === 0) {
            daysLeftTag = <span className="text-orange-400">À faire aujourd'hui</span>;
        } else {
            daysLeftTag = <span>jours restants : {diffDays}j</span>;
        }
    }

    const formatTargetWeek = (dateString: string) => {
        const d = new Date(dateString);
        return `Semaine du ${d.toLocaleDateString('fr-FR', { day: '2-digit', month: 'short' })}`;
    };

    return (
        <div className="bg-black-accent-default rounded-lg p-3 flex flex-col gap-3 transition-colors hover:bg-black-accent-dark">
            <div className="flex items-start gap-4">
                <button
                    onClick={handleToggle}
                    disabled={isPending}
                    className={`mt-1 w-4 h-4 rounded flex-shrink-0 border-2 transition-colors ${task.isChecked
                        ? 'bg-primary-default border-primary-default'
                        : 'bg-transparent border-gray-500 hover:border-gray-300'
                        }`}
                >
                    {task.isChecked && (
                        <svg viewBox="0 0 14 14" fill="none" className="w-full h-full text-white">
                            <path d="M3 7.5L5.5 10L11 4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                    )}
                </button>

                <div className="flex flex-col flex-1 min-w-0 gap-1">
                    <div className="flex items-center justify-between gap-4">
                        <span className={`text-sm font-medium truncate ${task.isChecked ? 'text-white-accent-dark line-through' : 'text-white-accent-light'}`}>
                            {task.name}
                        </span>

                        <div className="flex items-center gap-2 flex-shrink-0">
                            {difficultyInfo && (
                                <span className={`px-2 py-0.5 rounded text-[10px] border ${difficultyInfo.color}`}>
                                    {difficultyInfo.label} ({task.storyPoints} pts)
                                </span>
                            )}
                            {task.targetWeek && !task.isChecked && (
                                <span className="px-2 py-0.5 rounded text-[10px] bg-primary-default/10 text-primary-light border border-primary-default/20 flex items-center gap-1" title="Semaine assignée par l'algorithme">
                                    ✨ {formatTargetWeek(task.targetWeek)}
                                </span>
                            )}
                        </div>
                    </div>

                    <div className="flex items-center gap-4 text-xs mt-1">
                        <AssigneeDropdown task={task} projectId={projectId} />

                        <div className="flex items-center gap-2">
                            <DueDateSelector task={task} projectId={projectId} />
                            {daysLeftTag && (
                                <span className="text-[10px] text-white-accent-dark">
                                    ({daysLeftTag})
                                </span>
                            )}
                        </div>
                    </div>

                    {task.requiredDomains?.length > 0 && (
                        <div className="flex items-center gap-1 mt-1">
                            {task.requiredDomains.map(domain => (
                                <span key={domain} className="text-[9px] px-1.5 py-0.5 bg-white/5 text-white-accent-dark rounded">
                                    {domain}
                                </span>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};