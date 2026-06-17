import { Logo } from '@/components/atoms/Logo';
import { WorkspaceButton } from '@/components/atoms/WorkspaceButton';
import { useState } from 'react';

interface WorkspaceSidebarProps {
    onWorkspaceClick: (projectId: string) => void;
}

const MOCK_PROJECTS = [
    { id: 'proj_1', name: 'TaskForce', initials: 'TF' },
    { id: 'proj_2', name: 'Projet React', initials: 'PR' },
    { id: 'proj_3', name: 'Design System', initials: 'DS' },
];

export const WorkspaceSidebar = ({ onWorkspaceClick }: WorkspaceSidebarProps) => {
    const [activeProjectId, setActiveProjectId] = useState<string>('proj_1');

    const handleProjectClick = (projectId: string) => {
        setActiveProjectId(projectId);
        onWorkspaceClick(projectId);
    };
    return (
        <aside className="hidden w-[72px] shrink-0 flex-col items-center bg-black-accent-dark py-3 md:flex z-30">
            <WorkspaceButton
                variant="home"
                showIndicator={false}
                label="Accueil TaskForce"
            >
                <Logo size="sm" colorTheme="gradient" variant="icon-only" />
            </WorkspaceButton>

            <div className="mx-auto my-2 h-[6px] w-3 rounded-full bg-primary-dark" />

            {/* Projet "TaskForce" cliquable */}
            <div className="flex w-full flex-1 flex-col items-center space-y-2 overflow-y-auto no-scrollbar">

                {MOCK_PROJECTS.map((project) => (
                    <WorkspaceButton
                        key={project.id}
                        variant="project"
                        isActive={activeProjectId === project.id}
                        onClick={() => handleProjectClick(project.id)}
                        label={`Espace de travail ${project.name}`}
                    >
                        {project.initials}
                    </WorkspaceButton>
                ))}

            </div>

            <div className="mt-2 shrink-0">
                <WorkspaceButton
                    variant="action-success"
                    showIndicator={false}
                    label="Créer un nouveau projet"
                    onClick={() => console.log('Ouvrir la modal de création de projet')}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                    </svg>
                </WorkspaceButton>
            </div>
        </aside>
    );
};