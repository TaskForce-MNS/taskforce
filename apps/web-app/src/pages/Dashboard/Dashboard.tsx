import { Suspense, useState } from 'react';
import { useQuery, useSuspenseQuery } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/useAuthStore';
import { Button } from '@/components/atoms/Button';
import { ProjectCard } from '@/components/atoms/ProjectCard';
import { CreateProjectModal } from '@/templates/CreateProjectModal';
import { projectsQueryOptions } from '@/api/queries/projectsQueries';
import {
    WorkloadPanel,
    ProjectDistribution,
    ActivitySparkline,
    GrowthAreaChart,
    WeeklyRadarChart,
} from '@/components/statsBoard';

export const Dashboard = () => {
    const [isCreateOpen, setIsCreateOpen] = useState(false);

    const user = useAuthStore((state) => state.user);
    const isUserLoading = useAuthStore((state) => state.isLoading);
    const workload = user?.currentWorkload ?? 0;

    const { data: projects, isLoading: isProjectsLoading } = useQuery(projectsQueryOptions);
    const activeProjectsCount = projects?.length ?? 0;

    const latestProject = projects && projects.length > 0
        ? [...projects].sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())[0]
        : null;

    return (
        <div className="space-y-xl animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* En-tête de la page */}
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                    <p className="mt-1 font-text text-sm text-white-accent-dark">
                        Bienvenue sur ton tableau de bord, <strong className="text-white-accent-light">{user?.firstName}</strong> ! Ici tu peux suivre l'activité de tes projets et créer de nouveaux espaces pour organiser ton travail.
                    </p>
                </div>
            </div >

            {/* ── Statistiques ── */}
            < WorkloadPanel
                workload={workload}
                isUserLoading={isUserLoading}
                activeProjectsCount={activeProjectsCount}
                latestProject={latestProject}
                isProjectsLoading={isProjectsLoading}
            />

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <ProjectDistribution projects={projects} isLoading={isProjectsLoading} />
                <ActivitySparkline projects={projects} isLoading={isProjectsLoading} />
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <GrowthAreaChart projects={projects} isLoading={isProjectsLoading} />
                <WeeklyRadarChart projects={projects} isLoading={isProjectsLoading} />
            </div>

            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                    <h1 className="font-title text-xl font-bold text-white-accent-light">
                        Tout les projets
                    </h1>
                    {/* <p className="mt-1 font-text text-sm text-white-accent-dark">
                        Retrouve ici tous les projets sur lesquels tu travailles.
                    </p> */}
                </div>
                <Button variant="success" size="md" onClick={() => setIsCreateOpen(true)}>
                    Nouveau projet
                </Button>
            </div>
            {/* ── Grille de projets ── */}
            <Suspense fallback={<ProjectGridSkeleton />}>
                <ProjectContent onCreateClick={() => setIsCreateOpen(true)} />
            </Suspense>
            <CreateProjectModal
                isOpen={isCreateOpen}
                onClose={() => setIsCreateOpen(false)}
            />
        </div >
    );
};

function ProjectContent({ onCreateClick }: Readonly<{ onCreateClick: () => void }>) {
    const { data: projects } = useSuspenseQuery(projectsQueryOptions);

    if (projects.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center rounded-large border border-dashed border-white-accent-dark/30 bg-black-accent-dark/50 py-24 text-center">
                <span className="mb-4 text-4xl">🚀</span>
                <h3 className="mb-2 font-title text-xl font-semibold text-white-accent-light">
                    Aucun projet pour le moment
                </h3>
                <p className="mb-6 max-w-sm font-text text-sm text-white-accent-dark">
                    Crée ton premier projet pour commencer à organiser ton travail d'équipe.
                </p>
                <Button variant="primary" onClick={onCreateClick}>
                    Créer mon premier projet
                </Button>
            </div>
        );
    }

    return (
        <div>
            <h2 className="mb-4 font-title text-lg font-semibold text-white-accent-light">
                Tous les espaces
            </h2>
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
                {projects.map((project) => (
                    <ProjectCard key={project.id} project={project} />
                ))}
            </div>
        </div>
    );
}

function ProjectGridSkeleton() {
    return (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="h-32 animate-pulse rounded-medium bg-black-accent-default" />
            ))}
        </div>
    );
}