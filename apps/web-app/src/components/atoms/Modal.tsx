import { type ReactNode, useEffect } from 'react';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    children: ReactNode;
}

export const Modal = ({ isOpen, onClose, children }: ModalProps) => {
    // Empêche le scroll de la page quand la modale est ouverte
    useEffect(() => {
        if (isOpen) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'unset';
        }
        return () => {
            document.body.style.overflow = 'unset';
        };
    }, [isOpen]);

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
            {/* Arrière-plan flouté (cliquable pour fermer) */}
            <div
                className="absolute inset-0 bg-black/60 backdrop-blur-sm transition-opacity"
                onClick={onClose}
            />

            {/* Conteneur de la Modale */}
            <div className="relative w-full max-w-lg overflow-hidden rounded-2xl border border-white-accent-dark/20 bg-zinc-950 shadow-2xl">

                {/* Bouton Fermer */}
                <button
                    onClick={onClose}
                    className="absolute right-4 top-4 rounded-md p-1.5 text-white-accent-dark transition-colors hover:bg-white/10 hover:text-white"
                    title="Fermer"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>

                {/* Contenu */}
                <div className="p-2">
                    {children}
                </div>
            </div>
        </div>
    );
};