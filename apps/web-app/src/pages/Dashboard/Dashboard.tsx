// Dashboard.tsx
import { Suspense, useState } from 'react';
import { useSuspenseQuery } from '@tanstack/react-query';
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
import { JoinWorkspaceForm } from '@/components/molecules/JoinWorkspaceForm';

export const Dashboard = () => {
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const user = useAuthStore((state) => state.user);
    const isUserLoading = useAuthStore((state) => state.isLoading);
    const workload = user?.currentWorkload ?? 0;

    return (
        <div className="space-y-xl animate-in fade-in slide-in-from-bottom-4 duration-500">
            <JoinWorkspaceForm />
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <p className="mt-1 font-text text-sm text-white-accent-dark">
                    Bienvenue sur ton tableau de bord, <strong className="text-white-accent-light">{user?.firstName}</strong> !
                    Ici tu peux suivre l'activité de tes projets et créer de nouveaux espaces pour organiser ton travail.
                </p>
            </div>

            {/* ✅ Un seul point d'accès aux projets, tout le reste attend Suspense */}
            <Suspense fallback={<DashboardSkeleton />}>
                <DashboardContent
                    workload={workload}
                    isUserLoading={isUserLoading}
                    onCreateClick={() => setIsCreateOpen(true)}
                />
            </Suspense>

            <CreateProjectModal
                isOpen={isCreateOpen}
                onClose={() => setIsCreateOpen(false)}
            />
        </div>
    );
};

// ✅ Un seul useSuspenseQuery, tout le reste reçoit `projects` en props
function DashboardContent({
    workload,
    isUserLoading,
    onCreateClick,
}: {
    workload: number;
    isUserLoading: boolean;
    onCreateClick: () => void;
}) {
    const { data: projects } = useSuspenseQuery(projectsQueryOptions);
    const activeProjectsCount = projects.length;

    const latestProject = projects.length > 0
        ? [...projects].sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())[0]
        : null;

    return (
        <>
            <WorkloadPanel
                workload={workload}
                isUserLoading={isUserLoading}
                activeProjectsCount={activeProjectsCount}
                latestProject={latestProject}
                isProjectsLoading={false} // déjà résolu par Suspense ici
            />

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <ProjectDistribution projects={projects} isLoading={false} />
                <ActivitySparkline projects={projects} isLoading={false} />
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <GrowthAreaChart projects={projects} isLoading={false} />
                <WeeklyRadarChart projects={projects} isLoading={false} />
            </div>

            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <h1 className="font-title text-xl font-bold text-white-accent-light">
                    Tout les projets
                </h1>
                <Button variant="success" size="md" onClick={onCreateClick}>
                    Nouveau projet
                </Button>
            </div>

            {projects.length === 0 ? (
                <EmptyProjectsState onCreateClick={onCreateClick} />
            ) : (
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
            )}
        </>
    );
}

function EmptyProjectsState({ onCreateClick }: { onCreateClick: () => void }) {
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

function DashboardSkeleton() {
    return (
        <div className="space-y-4">
            <div className="h-32 animate-pulse rounded-medium bg-black-accent-default" />
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div className="h-40 animate-pulse rounded-medium bg-black-accent-default" />
                <div className="h-40 animate-pulse rounded-medium bg-black-accent-default" />
            </div>
        </div>
    );
}