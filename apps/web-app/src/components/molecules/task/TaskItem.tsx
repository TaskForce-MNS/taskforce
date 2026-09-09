import { type Task } from '@/api/task';
import { useUpdateTask } from '@/mutations/task';

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

    // Formatage de la date (On utilise CreatedAt pour l'instant)
    const formattedDate = new Intl.DateTimeFormat('fr-FR', {
        day: '2-digit',
        month: 'long',
        year: 'numeric'
    }).format(new Date(task.createdAt));

    return (
        <div className="bg-[#242424] border border-[#333333] rounded-lg p-4 flex flex-col gap-3 transition-colors hover:bg-[#2A2A2A]">

            {/* --- LIGNE SUPÉRIEURE --- */}
            <div className="flex items-center gap-4">
                {/* Checkbox circulaire */}
                <button
                    onClick={handleToggle}
                    disabled={isPending}
                    className={`w-5 h-5 rounded-full flex-shrink-0 border-2 transition-colors ${task.isChecked
                        ? 'bg-gray-400 border-gray-400' // Tâche terminée
                        : 'bg-transparent border-gray-400 hover:border-gray-200' // À faire
                        }`}
                />

                <div className="flex flex-1 items-center justify-between min-w-0 gap-4">

                    {/* Titre et Assignés */}
                    <div className="flex items-center gap-4 truncate">
                        <span className={`text-lg font-medium truncate ${task.isChecked ? 'text-gray-500 line-through' : 'text-gray-100'}`}>
                            {task.name}
                        </span>

                        {/* 🚧 MOCK: Assignation (À implémenter plus tard) */}
                        <div className="hidden md:flex items-center gap-2 text-sm">
                            <span className="text-gray-500">assigné à:</span>
                            <span className="bg-[#333333] text-gray-200 px-2 py-1 rounded-md text-xs">Léa J.</span>
                            <span className="bg-[#333333] text-gray-200 px-2 py-1 rounded-md text-xs">Martin O.</span>
                            <span className="bg-[#333333] text-gray-400 px-2 py-1 rounded-md text-xs">+ 2</span>
                        </div>
                    </div>

                    {/* Date et Priorité */}
                    <div className="flex items-center gap-4 flex-shrink-0">
                        <div className="flex items-center gap-2 text-gray-400 text-sm">
                            Logo calendar
                            <span>{formattedDate}</span>
                        </div>
                        {/* 🚧 MOCK: Priorité */}
                        <span className="bg-white text-black px-3 py-1 rounded text-sm font-medium">
                            Faible
                        </span>
                    </div>
                </div>
            </div>

            {/* --- LIGNE INFÉRIEURE (Décalée sur la droite) --- */}
            <div className="pl-9 flex items-center justify-between">

                {/* 🚧 MOCK: Indicateurs (Fichiers, Commentaires, Sous-tâches) */}
                <div className="flex items-center gap-2">
                    <div className="flex items-center gap-1.5 bg-[#1E1E1E] border border-[#333333] text-gray-400 px-2 py-1 rounded-md text-xs">
                        logo file <span>3</span>
                    </div>
                    <div className="flex items-center gap-1.5 bg-[#1E1E1E] border border-[#333333] text-gray-400 px-2 py-1 rounded-md text-xs">
                        logo message <span>12</span>
                    </div>
                    <div className="flex items-center gap-1.5 bg-[#1E1E1E] border border-[#333333] text-gray-400 px-2 py-1 rounded-md text-xs">
                        logo fork <span>4</span>
                    </div>
                </div>

                {/* 🚧 MOCK: Jours restants (Nécessitera une 'DueDate' en base plus tard) */}
                <div className="text-gray-400 text-sm bg-[#1E1E1E] px-3 py-1.5 rounded-md border border-[#333333]">
                    jours restant: 6j
                </div>
            </div>

        </div>
    );
};