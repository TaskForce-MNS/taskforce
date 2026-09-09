import { useState, Suspense, useCallback } from 'react';
import { Outlet, useNavigate, useParams } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Logo } from '@/components/atoms/Logo';
import { WorkspaceSidebar } from './WorkspaceSidebar';
import { CreateProjectModal } from './CreateProjectModal';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/components/atoms/Button';
import { projectQueryOptions } from '@/api/queries/projectsQueries';

export const DashboardLayout = () => {
    const navigate = useNavigate();
    const [isCreateProjectOpen, setIsCreateProjectOpen] = useState(false);

    const params = useParams({ strict: false }) as { projectId?: string };
    const projectId = params.projectId;

    const queryClient = useQueryClient();
    const logout = useAuthStore((state) => state.logout);
    const user = useAuthStore((state) => state.user);

    const { data: currentProject } = useQuery({
        ...projectQueryOptions(projectId || ''),
        enabled: !!projectId,
    });

    const displayName = user?.firstName || 'Utilisateur';
    const displayLastName = user?.lastName;
    const displayTitle = user?.title;
    const userInitial = `${displayName.charAt(0).toUpperCase()}${displayLastName?.charAt(0).toUpperCase()}`;

    const handleLogout = async () => {
        await logout();
        queryClient.clear();
        navigate({ to: '/auth' });
    };

    const handleCreateProjectClick = useCallback(() => {
        setIsCreateProjectOpen(true);
    }, []);


    const colorHex = currentProject?.colorHex ?? '#587B7F';
    const subtleBgColor = `${colorHex}20`;
    const projectInitials = currentProject?.name?.substring(0, 2).toUpperCase() || 'TF';

    return (
        <div className="flex h-screen w-full overflow-hidden bg-black-accent-light text-white-accent-default font-text">
            <Suspense fallback={<aside className="hidden w-[72px] shrink-0 bg-black-accent-dark md:flex z-30" />}>
                <WorkspaceSidebar onCreateProjectClick={handleCreateProjectClick} />
            </Suspense>
            <div className="flex min-w-0 flex-1 flex-col">
                <header className="flex h-14 shrink-0 items-center overflow-hidden border-b border-black-accent-light/50 px-4 sm:px-6 z-10 bg-black-accent-dark backdrop-blur-md m-1 rounded-lg shadow-sm">
                    <div className="flex items-center gap-3">
                        <div className="block sm:hidden">
                            <Logo variant="icon-only" size="sm" />
                        </div>

                        <div className="hidden sm:block">
                            <Logo variant="text-only" size="sm" colorTheme="currentColor" />
                        </div>

                        {currentProject && (
                            <>
                                <span className="text-white-accent-dark/40">/</span>
                                <div className="relative flex items-center justify-center p-1">
                                    <div
                                        className="absolute inset-0 -inset-x-5 -inset-y-5  rounded-full blur-sm"
                                        style={{ backgroundColor: subtleBgColor }}
                                        aria-hidden="true"
                                    />
                                    <div className="relative z-10 flex items-center gap-2 rounded-md bg-white-accent-dark/5 px-2 py-1">
                                        <div
                                            className="flex h-5 w-5 items-center justify-center rounded font-title text-[10px] font-bold text-white"
                                            style={{ backgroundColor: currentProject.colorHex ?? '#587B7F' }}
                                        >
                                            {projectInitials}
                                        </div>
                                        <span className="font-title text-sm font-semibold text-white-accent-light">
                                            {currentProject.name}
                                        </span>
                                    </div>
                                </div>
                            </>
                        )}
                    </div>

                    <div className="ml-auto flex items-center gap-2 sm:gap-4">
                        <div className="flex items-center gap-3 rounded-xl border border-white-accent-dark/15 bg-black-accent-light/50 p-1 shadow-inner transition-all hover:border-primary-default/50 sm:pr-4">
                            <div className="relative shrink-0">
                                <div className="flex h-7 w-7 items-center justify-center rounded-xl bg-gradient-to-tr from-indigo-600 to-primary-default font-title text-xs font-bold text-white shadow-sm">
                                    {userInitial}
                                </div>
                                <span className="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full border-2 border-black-accent-light bg-emerald-500" title="En ligne" />
                            </div>
                            <div className="hidden flex-col text-left sm:flex">
                                <span className="max-w-[120px] truncate font-title text-xs font-bold leading-tight text-white-accent-light">
                                    {displayName} {displayLastName}
                                </span>
                                <span className="text-[10px] leading-tight text-white-accent-dark">
                                    {displayTitle}
                                </span>
                            </div>
                        </div>

                        <Button
                            variant='outline'
                            radius='lg'
                            onClick={handleLogout}
                            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full border border-white-accent-dark/10 bg-black-accent-light/30 text-white-accent-dark transition-all hover:border-red-500/30 hover:bg-red-500/10 hover:text-red-400"
                            title="Se déconnecter"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.8} stroke="currentColor" className="h-4 w-4">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15M12 9l-3 3m0 0 3 3m-3-3h12.75" />
                            </svg>
                        </Button>
                    </div>
                </header>

                <main className="min-w-0 flex-1 overflow-y-auto overflow-x-hidden p-2 lg:p-4">
                    <Suspense fallback={<div className="p-4 text-white-accent-dark">Chargement...</div>}>
                        <Outlet />
                    </Suspense>
                </main>
            </div >

            <CreateProjectModal
                isOpen={isCreateProjectOpen}
                onClose={() => setIsCreateProjectOpen(false)}
            />
        </div >
    );
};