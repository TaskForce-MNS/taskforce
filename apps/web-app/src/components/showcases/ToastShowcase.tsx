import { useToastStore } from '@/stores/useToastStore';
import { Button } from '@/components/atoms/Button';

const TOAST_DEMOS = [
    {
        variant: 'success' as const,
        title: 'Connexion réussie',
        message: 'Bienvenue sur votre espace Taskforce.',
        label: 'Tester le Succès',
        buttonVariant: 'primary' as const,
    },
    {
        variant: 'error' as const,
        title: 'Échec de la sauvegarde',
        message: 'Impossible de joindre la base de données.',
        label: "Tester l'Erreur",
        buttonVariant: 'danger' as const,
    },
    {
        variant: 'info' as const,
        title: 'Mise à jour',
        message: "Une nouvelle version de l'API est disponible.",
        label: "Tester l'Info",
        buttonVariant: 'secondary' as const,
    },
    {
        variant: 'warning' as const,
        title: 'Attention',
        message: 'Cette action supprimera définitivement le dossier.',
        label: "Tester l'Avertissement",
        buttonVariant: 'outline' as const,
    },
];

export const ToastShowcase = ({ label }: { label: string }) => {
    const addToast = useToastStore((state) => state.addToast);

    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                5. {label}
            </h2>

            <p className="text-sm text-white-accent-dark/80 mb-6">
                Cliquez sur les boutons ci-dessous pour déclencher l'état global via Zustand.
                Les toasts apparaîtront en bas à droite et disparaîtront après 5 secondes.
            </p>

            <div className="flex flex-wrap gap-4">
                {TOAST_DEMOS.map(({ variant, title, message, label, buttonVariant }) => (
                    <Button
                        key={variant}
                        variant={buttonVariant}
                        onClick={() => addToast({ variant, title, message })}
                    >
                        {label}
                    </Button>
                ))}

                <Button
                    variant="outline"
                    onClick={() => addToast({
                        variant: 'success',
                        title: 'Copié dans le presse-papier !',
                        duration: 2_000,
                    })}
                >
                    Toast Rapide (2s)
                </Button>
            </div>
        </section>
    );
};