import { useState } from 'react';
import { Outlet, useNavigate } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Logo } from '@/components/atoms/Logo';
import { WorkspaceSidebar } from './WorkspaceSidebar';

export const DashboardLayout = () => {
    const logout = useAuthStore((state) => state.logout);
    const navigate = useNavigate();
    // L'état qui gère l'ouverture/fermeture du menu
    const [isMenuOpen, setIsMenuOpen] = useState(false);

    const handleLogout = async () => {
        await logout();
        navigate({ to: '/auth' });
    };

    return (
        // 1. Conteneur principal 
        <div className="flex h-screen w-full bg-black-accent-light text-white-accent-default overflow-hidden font-text">

            <WorkspaceSidebar
                onWorkspaceClick={() => setIsMenuOpen(!isMenuOpen)}
            />

            {/* 2. BARRE LATÉRALE INTERNE (Affichée uniquement si isMenuOpen est true) */}
            {isMenuOpen && (
                <aside className="hidden w-60 shrink-0 flex-col bg-black-accent-default md:flex z-20 transition-all">

                    {/* --- Colle ici tout le contenu actuel de ton aside "Menu / Salons" --- */}
                    {/* ... (En-tête "TaskForce", nav custom-scrollbar, Zone Profil Utilisateur) ... */}
                    {/* N'oublie pas de garder ton onClick={handleLogout} sur le bouton de déconnexion ! */}

                </aside>
            )}

            {/* 5. ─── ZONE PRINCIPALE (Prend le reste de l'écran) ─── */}
            <div className="flex min-w-0 flex-1 flex-col">

                {/* EN-TÊTE HAUT DU SALON (Header) */}
                <header className="flex h-12 shrink-0 items-center justify-between border-b border-black-accent-default px-4 z-10 shadow-sm">
                    <div className="flex items-center gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="size-6 text-white-accent-dark">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M5.25 8.25h15m-16.5 7.5h15m-1.8-13.5-3.9 19.5m-2.1-19.5-3.9 19.5" />
                        </svg>
                        <h2 className="font-title text-base font-bold text-white-accent-light">
                            vue-d-ensemble
                        </h2>
                    </div>
                </header>

                {/* LE CONTENU DE LA PAGE EN COURS */}
                <main className="flex-1 overflow-y-auto p-l lg:p-xl">
                    <Outlet />
                </main>

            </div>
        </div>
    );
}