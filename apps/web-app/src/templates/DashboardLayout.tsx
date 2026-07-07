import { useState } from 'react';
import { Outlet, useNavigate } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Logo } from '@/components/atoms/Logo';
import { WorkspaceSidebar } from './WorkspaceSidebar';

export const DashboardLayout = () => {
    const navigate = useNavigate();
    const [isMenuOpen, setIsMenuOpen] = useState(false);

    const logout = useAuthStore((state) => state.logout);

    const handleLogout = async () => {
        await logout(); // ← le store appelle l'API et met isAuthenticated à false
        navigate({ to: '/auth' });
    };

    return (
        <div className="flex h-screen w-full bg-black-accent-light text-white-accent-default overflow-hidden font-text">
            <WorkspaceSidebar
                onWorkspaceClick={() => setIsMenuOpen(!isMenuOpen)}
            />

            {/* BARRE LATÉRALE INTERNE */}
            {isMenuOpen && (
                <aside className="hidden w-60 shrink-0 flex-col bg-black-accent-default md:flex z-20 transition-all border-r border-black-accent-light">

                    {/* En-tête de la barre latérale */}
                    <div className="flex h-12 items-center px-4 border-b border-black-accent-light shadow-sm">
                        <h2 className="font-title font-bold text-white-accent-light">TaskForce</h2>
                    </div>

                    {/* Zone des menus (Scrollable) */}
                    <nav className="flex-1 overflow-y-auto p-3 custom-scrollbar">
                        {/* Tes liens de navigation iront ici */}
                    </nav>

                    {/* Zone Profil Utilisateur et Déconnexion (Tout en bas) */}
                    <div className="mt-auto bg-black-accent-dark flex items-center justify-between p-3 border-t border-black-accent-light">
                        <div className="flex items-center gap-3 overflow-hidden">
                            {/* Faux Avatar */}
                            <div className="w-8 h-8 rounded-full bg-indigo-600 flex items-center justify-center font-bold text-sm shrink-0">
                                U
                            </div>
                            <div className="flex flex-col truncate">
                                <span className="text-sm font-bold truncate">Utilisateur</span>
                                <span className="text-xs text-white-accent-dark truncate">En ligne</span>
                            </div>
                        </div>

                        {/* Bouton Logout */}
                        <button
                            onClick={handleLogout}
                            className="p-2 rounded-md text-white-accent-dark hover:bg-black-accent-light hover:text-red-400 transition-colors"
                            title="Se déconnecter"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15M12 9l-3 3m0 0 3 3m-3-3h12.75" />
                            </svg>
                        </button>
                    </div>
                </aside>
            )}

            {/* ZONE PRINCIPALE */}
            <div className="flex min-w-0 flex-1 flex-col">
                {/* EN-TÊTE HAUT DU SALON (Header) */}
                <header className="flex h-12 shrink-0 items-center justify-between border-b border-black-accent-default px-4 z-10 shadow-sm bg-black-accent-light">
                    <div className="flex items-center gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-6 h-6 text-white-accent-dark">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M5.25 8.25h15m-16.5 7.5h15m-1.8-13.5-3.9 19.5m-2.1-19.5-3.9 19.5" />
                        </svg>
                        <h2 className="font-title text-base font-bold text-white-accent-light">
                            vue-d-ensemble
                        </h2>
                    </div>
                </header>

                {/* LE CONTENU DE LA PAGE EN COURS */}
                <main className="flex-1 overflow-y-auto p-4 lg:p-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};