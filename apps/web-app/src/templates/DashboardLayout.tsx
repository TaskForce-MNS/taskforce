import { useState, Suspense } from 'react';
import { Outlet, useNavigate } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Logo } from '@/components/atoms/Logo';
import { WorkspaceSidebar } from './WorkspaceSidebar';
import { CreateProjectModal } from './CreateProjectModal';
import { useQueryClient } from '@tanstack/react-query';

export const DashboardLayout = () => {
    const navigate = useNavigate();
    const [isCreateProjectOpen, setIsCreateProjectOpen] = useState(false);

    const queryClient = useQueryClient();
    const logout = useAuthStore((state) => state.logout);
    const user = useAuthStore((state) => state.user);

    const displayName = user?.firstname || 'Utilisateur';
    const displayLastName = user?.lastname;
    const displayTitle = user?.title;
    const userInitial = `${displayName.charAt(0).toUpperCase()}${displayLastName?.charAt(0).toUpperCase()}`;

    console.log('DashboardLayout rendered with user:', user);
    const handleLogout = async () => {
        await logout();
        queryClient.clear();
        navigate({ to: '/auth' });
    };

    return (
        <div className="flex h-screen w-full bg-black-accent-light text-white-accent-default overflow-hidden font-text">
            <WorkspaceSidebar
                onCreateProjectClick={() => setIsCreateProjectOpen(true)}
            />

            {/* ZONE PRINCIPALE — inchangée */}
            <div className="flex min-w-0 flex-1 flex-col">
                <header className="flex h-14 shrink-0 items-center border-b border-black-accent-light/50 px-4 sm:px-6 z-10 bg-black-accent-dark backdrop-blur-md">
                    <div className="flex items-center gap-3">
                        <Logo variant="text-only" size="sm" />
                    </div>

                    <div className="ml-auto flex items-center gap-2 sm:gap-4">
                        <div className="flex items-center gap-3 rounded-xl border border-white-accent-dark/15 bg-black-accent-light/50 p-1 sm:pr-4 shadow-inner transition-all hover:border-primary-default/50">
                            <div className="relative shrink-0">
                                <div className="flex h-7 w-7 items-center justify-center rounded-full bg-gradient-to-tr from-indigo-600 to-primary-default font-title text-xs font-bold text-white shadow-sm">
                                    {userInitial}
                                </div>
                                <span className="absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full border-2 border-black-accent-light bg-emerald-500" title="En ligne" />
                            </div>
                            <div className="hidden sm:flex flex-col text-left">
                                <span className="font-title text-xs font-bold leading-tight text-white-accent-light truncate max-w-[120px]">
                                    {displayName} {displayLastName}
                                </span>
                                <span className="text-[10px] leading-tight text-white-accent-dark">
                                    {displayTitle}
                                </span>
                            </div>
                        </div>

                        <button
                            onClick={handleLogout}
                            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full border border-white-accent-dark/10 bg-black-accent-light/30 text-white-accent-dark transition-all hover:border-red-500/30 hover:bg-red-500/10 hover:text-red-400"
                            title="Se déconnecter"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.8} stroke="currentColor" className="h-4 w-4">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15M12 9l-3 3m0 0 3 3m-3-3h12.75" />
                            </svg>
                        </button>
                    </div>
                </header>

                <main className="flex-1 overflow-y-auto p-4 lg:p-8">
                    <Suspense fallback={<div className="p-4 text-white-accent-dark">Chargement...</div>}>
                        <Outlet />
                    </Suspense>
                </main>
            </div>

            <CreateProjectModal
                isOpen={isCreateProjectOpen}
                onClose={() => setIsCreateProjectOpen(false)}
            />
        </div>
    );
};