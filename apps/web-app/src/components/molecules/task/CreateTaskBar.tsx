import { useState, useRef, useEffect, type SyntheticEvent, type KeyboardEvent } from 'react';
import { useCreateTask } from '@/mutations/task';
import { TaskDifficulty } from '@/api/task';

interface CreateTaskBarProps {
    projectId: string;
    projectName?: string;
}

const MOCK_TAGS: string[] = [
    // "Date de début",
    // "Date de fin",
    // "Nb personnes"
];

export const CreateTaskBar = ({ projectId, projectName }: CreateTaskBarProps) => {

    const [isExpanded, setIsExpanded] = useState(false);
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');

    const [showAlgoSettings, setShowAlgoSettings] = useState(false);
    const [difficulty, setDifficulty] = useState<TaskDifficulty>(TaskDifficulty.None);
    const [requiredDomains, setRequiredDomains] = useState<string[]>([]);
    const [domainInput, setDomainInput] = useState('');
    const [dueDate, setDueDate] = useState<string>('');

    const titleInputRef = useRef<HTMLInputElement>(null);
    const descTextareaRef = useRef<HTMLTextAreaElement>(null);

    const { mutate: createTask, isPending } = useCreateTask(projectId);

    useEffect(() => {
        if (isExpanded) {
            descTextareaRef.current?.focus();
        }
    }, [isExpanded]);

    const handleDomainKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter') {
            e.preventDefault(); 
            const newDomain = domainInput.trim();
            if (newDomain && !requiredDomains.includes(newDomain)) {
                setRequiredDomains([...requiredDomains, newDomain]);
            }
            setDomainInput('');
        }
    };

    const removeDomain = (domainToRemove: string) => {
        setRequiredDomains(requiredDomains.filter(d => d !== domainToRemove));
    };

    const handleSubmit = (e: SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!title.trim() || isPending) return;

        createTask(
            {
                name: title.trim(),
                description: description.trim() || null,
                projectId,
                difficulty: difficulty !== TaskDifficulty.None ? difficulty : undefined,
                requiredDomains: requiredDomains.length > 0 ? requiredDomains : undefined,
                dueDate: dueDate ? new Date(dueDate).toISOString() : undefined
            },
            {
                onSuccess: () => {
                    setTitle('');
                    setDescription('');
                    setDifficulty(TaskDifficulty.None);
                    setRequiredDomains([]);
                    setDueDate('');
                    setShowAlgoSettings(false);
                    setIsExpanded(false);
                    titleInputRef.current?.focus();
                }
            }
        );
    };

    const renderSubmitButton = (fullWidth: boolean = false) => (
        <button
            type="submit"
            disabled={!title.trim() || isPending}
            className={`bg-secondary-default hover:bg-secondary-dark text-white-accent-light px-4 py-2 rounded-medium text-xs font-medium transition-colors disabled:bg-secondary-light disabled:text-white-accent-dark disabled:cursor-not-allowed flex items-center justify-center gap-2 ${fullWidth ? 'w-full' : ''}`}
        >
            {isPending && <span className="animate-pulse">...</span>}
            Créer la tâche
        </button>
    );

    const ToggleButton = (
        <button
            type="button"
            onClick={() => setIsExpanded(!isExpanded)}
            className="bg-white-accent-light text-black-accent-dark h-full w-6 flex items-center justify-center rounded-medium transition-colors shrink-0"
        >
            <span className={`transition-transform duration-300 ${isExpanded ? 'rotate-45' : ''}`}>+</span>
        </button>
    );

    return (
        <form
            onSubmit={handleSubmit}
            className="bg-black-accent-dark rounded-medium p-2 flex flex-col gap-3 shadow-lg w-full"
        >
            <div className="flex flex-wrap gap-2">
                <button
                    type="button"
                    onClick={() => setShowAlgoSettings(!showAlgoSettings)}
                    className={`${showAlgoSettings ? 'bg-primary-default text-white' : 'bg-[#333333] text-gray-300 hover:bg-[#404040]'
                        } px-3 py-1.5 rounded-large text-xs transition-colors flex items-center gap-1`}
                >
                    ✨ Assignation IA
                    {(requiredDomains.length > 0 || difficulty !== TaskDifficulty.None) && (
                        <span className="bg-white/20 px-1.5 rounded-full text-[10px] ml-1">Actif</span>
                    )}
                </button>

                {MOCK_TAGS.map((tag, index) => (
                    <button
                        key={index}
                        type="button"
                        className="bg-[#333333] hover:bg-[#404040] text-gray-300 px-3 py-1.5 rounded-large text-xs transition-colors"
                    >
                        {tag}
                    </button>
                ))}
            </div>

            {showAlgoSettings && (
                <div className="flex flex-col sm:flex-row gap-3 bg-black-accent-light/30 p-3 rounded-md border border-primary-default/20 mb-2">
                    <div className="flex flex-col gap-1 w-full sm:w-1/3">
                        <label className="text-[10px] uppercase font-bold text-white-accent-dark">Difficulté</label>
                        <select
                            value={difficulty}
                            onChange={(e) => setDifficulty(Number(e.target.value) as TaskDifficulty)}
                            className="bg-[#2A2A2A] text-white px-2 py-1.5 rounded border border-[#404040] focus:border-primary-default focus:outline-none text-xs"
                        >
                            <option value={TaskDifficulty.None}>Non définie</option>
                            <option value={TaskDifficulty.Simple}>Simple (1 pt)</option>
                            <option value={TaskDifficulty.Medium}>Moyenne (3 pts)</option>
                            <option value={TaskDifficulty.Complex}>Complexe (8 pts)</option>
                        </select>
                    </div>

                    <div className="flex flex-col gap-1 w-full sm:w-2/3">
                        <label className="text-[10px] uppercase font-bold text-white-accent-dark">
                            Domaines requis (Entrée pour ajouter)
                        </label>
                        <div className="flex flex-wrap gap-1 mb-1">
                            {requiredDomains.map(domain => (
                                <span key={domain} className="bg-primary-default/20 text-primary-light px-2 py-0.5 rounded text-[10px] flex items-center gap-1">
                                    {domain}
                                    <button type="button" onClick={() => removeDomain(domain)} className="hover:text-white">✕</button>
                                </span>
                            ))}
                        </div>
                        <input
                            type="text"
                            value={domainInput}
                            onChange={(e) => setDomainInput(e.target.value)}
                            onKeyDown={handleDomainKeyDown}
                            placeholder="Ex: React, C#..."
                            className="bg-[#2A2A2A] text-white px-2 py-1.5 rounded border border-[#404040] focus:border-primary-default focus:outline-none text-xs"
                        />
                    </div>
                </div>
            )}

            <div className="flex flex-col gap-2 w-full">
                <div className="flex items-center gap-2 w-full">
                    {!isExpanded && ToggleButton}
                    <input
                        ref={titleInputRef}
                        type="text"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        placeholder={isExpanded ? "Titre de la tâche" : `Créer une tâche dans ${projectName}`}
                        className="flex-1 bg-[#2A2A2A] text-white px-4 py-2 rounded-medium border border-transparent focus:border-[#404040] focus:outline-none placeholder-gray-500 text-sm"
                    />
                    {!isExpanded && renderSubmitButton()}
                </div>

                {isExpanded && (
                    <div className="flex items-start gap-2 w-full">
                        {ToggleButton}
                        <div className="flex-1 flex flex-col gap-3">
                            <textarea
                                ref={descTextareaRef}
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                placeholder="Ajouter une description..."
                                rows={4}
                                className="w-full bg-[#2A2A2A] text-white px-4 py-3 rounded-md border border-transparent focus:border-[#404040] focus:outline-none placeholder-gray-500 text-sm resize-none"
                            />
                            <div className="flex items-center gap-2">
                                <label className="text-xs text-white-accent-dark">Échéance :</label>
                                <input
                                    type="date"
                                    value={dueDate}
                                    onChange={(e) => setDueDate(e.target.value)}
                                    className="bg-[#2A2A2A] text-white px-2 py-1.5 rounded border border-[#404040] focus:border-primary-default focus:outline-none text-xs"
                                />
                            </div>
                            <div className="flex w-full">
                                {renderSubmitButton(true)}
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </form>
    );
};