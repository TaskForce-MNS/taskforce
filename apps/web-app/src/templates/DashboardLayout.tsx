import { Outlet, useNavigate } from '@tanstack/react-router';
import { useAuthStore } from '@/stores/useAuthStore';
import { Button } from '@/components/atoms/Button';
import { Logo } from '@/components/atoms/Logo';

export const DashboardLayout = () => {
    const logout = useAuthStore((state) => state.logout);
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        navigate({ to: '/auth' });
    };

    return (
        // 1. Conteneur principal (prend tout l'écran)
        <div className="flex h-screen w-full bg-black-accent-dark text-white-accent-light overflow-hidden font-text">

            {/* 2. ─── BARRE LATÉRALE GAUCHE (Sidebar) ─── */}
            <aside className="hidden w-64 flex-col border-r border-white-accent-dark/10 bg-black-accent-default p-m md:flex z-20 shadow-2xl">

                {/* Logo de l'application */}
                <div className="mb-8 flex items-center px-2">
                    <Logo size="md" colorTheme="gradient" />
                </div>

                {/* Navigation principale */}
                <nav className="flex-1 space-y-2">
                    {/* Exemple de bouton "Actif" */}
                    <div className="cursor-pointer rounded-medium bg-primary-default/10 px-4 py-2.5 text-sm font-semibold text-primary-default transition-colors">
                        Tableau de bord
                    </div>

                    {/* Exemple de bouton "Inactif" */}
                    <div className="cursor-pointer rounded-medium px-4 py-2.5 text-sm font-medium text-white-accent-dark hover:bg-white-accent-dark/10 hover:text-white-accent-light transition-colors">
                        Mes Projets
                    </div>
                    <div className="cursor-pointer rounded-medium px-4 py-2.5 text-sm font-medium text-white-accent-dark hover:bg-white-accent-dark/10 hover:text-white-accent-light transition-colors">
                        Paramètres
                    </div>
                </nav>

                {/* Zone bas de page : Déconnexion */}
                <div className="mt-auto pt-4 border-t border-white-accent-dark/10">
                    <Button
                        variant="link"
                        onClick={handleLogout}
                        className="w-full justify-start text-white-accent-dark hover:text-error text-sm font-medium px-4 py-2"
                    >
                        Se déconnecter
                    </Button>
                </div>
            </aside>

            {/* 3. ─── ZONE PRINCIPALE (Prend le reste de l'écran) ─── */}
            <div className="flex flex-1 flex-col min-w-0">

                {/* 4. EN-TÊTE HAUT (Header) */}
                <header className="flex h-16 shrink-0 items-center justify-between border-b border-white-accent-dark/10 bg-black-accent-default/50 px-6 backdrop-blur-md z-10">
                    <h2 className="font-title text-lg font-semibold text-white-accent-light">
                        Vue d'ensemble
                    </h2>

                    <div className="flex items-center gap-4">
                        {/* Bulle Avatar Utilisateur */}
                        <div className="flex items-center gap-3 cursor-pointer rounded-full p-1 pr-3 hover:bg-white-accent-dark/10 transition-colors">
                            <div className="h-8 w-8 rounded-full bg-gradient-to-tr from-primary-default to-secondary-default shadow-inner" />
                            <span className="text-sm font-medium text-white-accent-light hidden sm:block">
                                Mon Profil
                            </span>
                        </div>
                    </div>
                </header>

                {/* 5. LE CONTENU DE LA PAGE EN COURS */}
                {/* L'Outlet de TanStack Router va "injecter" ici le contenu du dashboard */}
                <main className="flex-1 overflow-y-auto p-l lg:p-xl">
                    <Outlet />
                </main>

            </div>
        </div>
    );
}