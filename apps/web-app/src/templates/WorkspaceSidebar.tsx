import { Logo } from '@/components/atoms/Logo';
import { WorkspaceButton } from '@/components/atoms/WorkspaceButton';
import { useSuspenseQuery } from '@tanstack/react-query';
import { useMatchRoute, useNavigate } from '@tanstack/react-router';
import { projectsQueryOptions } from '@/api/queries/projectsQueries';

interface WorkspaceSidebarProps {
    onCreateProjectClick: () => void;
}

export const WorkspaceSidebar = ({ onCreateProjectClick }: WorkspaceSidebarProps) => {
    const { data: projects } = useSuspenseQuery(projectsQueryOptions);
    const matchRoute = useMatchRoute();
    const navigate = useNavigate();

    const isDashboardActive = !!matchRoute({ to: '/dashboard' });
    const activeProjectMatch = matchRoute({ to: '/projects/$projectId', fuzzy: true });
    const activeProjectId = activeProjectMatch ? activeProjectMatch.projectId : null;

    return (
        <aside className="hidden w-[72px] shrink-0 flex-col items-center bg-black-accent-dark py-3 md:flex z-30">
            <WorkspaceButton
                variant="home"
                showIndicator={true}
                isActive={isDashboardActive}
                label="Accueil TaskForce"
                onClick={() => navigate({ to: '/dashboard' })}
            >
                <Logo size="sm" colorTheme="gradient" variant="icon-only" />
            </WorkspaceButton>

            <div className="mx-auto my-2 h-[6px] w-3 rounded-full bg-primary-dark" />

            <div className="flex w-full flex-1 flex-col items-center space-y-2 overflow-y-auto no-scrollbar">
                {projects.map((project) => {
                    const initial = project.name.charAt(0).toUpperCase();

                    return (
                        <WorkspaceButton
                            key={project.id}
                            variant="project"
                            isActive={activeProjectId === project.id}
                            onClick={() => navigate({ to: '/projects/$projectId', params: { projectId: project.id } })}
                            label={`Espace de travail ${project.name}`}
                        >
                            {initial}
                        </WorkspaceButton>
                    );
                })}
            </div>

            <div className="mt-2 shrink-0">
                <WorkspaceButton
                    variant="action-success"
                    showIndicator={false}
                    label="Créer un nouveau projet"
                    onClick={onCreateProjectClick}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                    </svg>
                </WorkspaceButton>
            </div>
        </aside>
    );
}