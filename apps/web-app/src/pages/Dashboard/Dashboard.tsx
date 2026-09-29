// Dashboard.tsx
import { Suspense, useMemo, useState } from 'react';
import { useSuspenseQuery, useQueries } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/useAuthStore';
import { Button } from '@/components/atoms/Button';
import { ProjectCard } from '@/components/atoms/ProjectCard';
import { CreateProjectModal } from '@/templates/CreateProjectModal';
import { projectsQueryOptions } from '@/api/queries/projectsQueries';
import {
    WorkloadPanel,
    ProjectDistribution,
    ActivitySparkline,
    TaskDifficultyRadar,
    WorkloadAreaChart
} from '@/components/statsBoard';
import { JoinWorkspaceForm } from '@/components/molecules/JoinWorkspaceForm';
import { tasksApi } from '@/api/task';

type DashboardUser = ReturnType<typeof useAuthStore.getState>['user'];

export const Dashboard = () => {
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const user = useAuthStore((state) => state.user);
    const isUserLoading = useAuthStore((state) => state.isLoading);

    return (
        <div className="space-y-xl animate-in fade-in slide-in-from-bottom-4 duration-500">
            <JoinWorkspaceForm />
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <p className="mt-1 font-text text-sm text-white-accent-dark">
                    Bienvenue sur ton tableau de bord, <strong className="text-white-accent-light">{user?.firstName}</strong> !
                    Ici tu peux suivre l'activité de tes projets et créer de nouveaux espaces pour organiser ton travail.
                </p>
            </div>

            <Suspense fallback={<DashboardSkeleton />}>
                <DashboardContent
                    user={user}
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

function DashboardContent({
    user,
    isUserLoading,
    onCreateClick,
}: {
    user: DashboardUser;
    isUserLoading: boolean;
    onCreateClick: () => void;
}) {
    const { data: projects } = useSuspenseQuery(projectsQueryOptions);
    const activeProjectsCount = projects.length;

    const latestProject = projects.length > 0
        ? [...projects].sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())[0]
        : null;
    const taskQueries = useQueries({
        queries: projects.map((project) => ({
            queryKey: ['projects', project.id, 'tasks'],
            queryFn: () => tasksApi.listForProject(project.id),
        })),
    });

    const isTasksLoading = taskQueries.some((q) => q.isLoading);
    const { workloadPoints, upcomingDeadlinesCount, projectWorkloads } = useMemo(() => {
        if (!user) return { workloadPoints: 0, upcomingDeadlinesCount: 0, projectWorkloads: {} };

        const now = new Date();
        const day = now.getDay() || 7;
        const currentMonday = new Date(now);
        currentMonday.setDate(now.getDate() - day + 1);

        const toYMD = (date: Date | string) => {
            const d = new Date(date);
            return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
        };
        const currentMondayYMD = toYMD(currentMonday);

        let globalPoints = 0;
        let globalDeadlines = 0;
        const workloads: Record<string, number> = {};

        taskQueries.forEach(({ data: tasks }, index) => {
            const projectId = projects[index].id;
            let projectPoints = 0;

            if (tasks) {
                tasks.forEach((task) => {
                    const isMyTask = task.assigneeId?.toLowerCase() === user.id.toLowerCase();

                    if (isMyTask && !task.isChecked) {
                        if (task.targetWeek) {
                            if (toYMD(task.targetWeek) === currentMondayYMD) {
                                projectPoints += (task.storyPoints || 0);
                            }
                        } else {
                            projectPoints += (task.storyPoints || 0);
                        }

                        if (task.dueDate) {
                            const diffTime = new Date(task.dueDate).getTime() - now.getTime();
                            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                            if (diffDays <= 3) globalDeadlines++;
                        }
                    }
                });
            }

            workloads[projectId] = projectPoints;
            globalPoints += projectPoints;
        });

        return {
            workloadPoints: globalPoints,
            upcomingDeadlinesCount: globalDeadlines,
            projectWorkloads: workloads
        };
    }, [taskQueries, user, projects]);

    const allUserTasks = useMemo(() => {
        if (!user) return [];
        return taskQueries
            .flatMap((q) => q.data || [])
            .filter((task) => task.assigneeId?.toLowerCase() === user.id.toLowerCase());
    }, [taskQueries, user]);

    return (
        <>
            <WorkloadPanel
                workloadPoints={workloadPoints}
                upcomingDeadlinesCount={upcomingDeadlinesCount}
                isUserLoading={isUserLoading || isTasksLoading}
                activeProjectsCount={activeProjectsCount}
                latestProject={latestProject}
                isProjectsLoading={false}
            />

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <ProjectDistribution projects={projects} isLoading={false} />
                <ActivitySparkline projects={projects} isLoading={false} />
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 mt-4">
                <WorkloadAreaChart tasks={allUserTasks} isLoading={isTasksLoading} />
                <TaskDifficultyRadar tasks={allUserTasks} isLoading={isTasksLoading} />
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
                        {projects.map((project) => {
                            const projectPoints = projectWorkloads[project.id] || 0;
                            const MAX_POINTS_PER_WEEK = 20;
                            const projectPercentage = Math.round(Math.min((projectPoints / MAX_POINTS_PER_WEEK) * 100, 100));

                            return (
                                <div key={project.id} className="flex flex-col gap-2">
                                    <div className="flex items-center justify-end px-1">
                                        <span className="flex items-center gap-1.5 rounded-md bg-black-accent-light px-2 py-1 font-text text-xs text-white-accent-dark">
                                            Ma charge :
                                            <strong className={projectPercentage > 0 ? "text-primary-default" : "text-white-accent-dark/50"}>
                                                {projectPercentage}%
                                            </strong>
                                        </span>
                                    </div>

                                    <ProjectCard project={project} />
                                </div>
                            );
                        })}
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