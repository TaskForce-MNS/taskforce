import { type ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { Logo } from '@/components/atoms/Logo';

interface LegalPageTemplateProps {
    title: string;
    lastUpdated: string;
    children: ReactNode;
}

export const LegalPageTemplate = ({ title, lastUpdated, children }: LegalPageTemplateProps) => {
    return (
        <div className="min-h-screen bg-black-accent-light px-4 py-12 font-text text-white-accent-default sm:px-6 lg:px-8">
            <div className="mx-auto max-w-3xl">
                <div className="mb-12 flex items-center justify-between">
                    <Link to="/auth" className="flex items-center gap-2 transition-opacity hover:opacity-80">
                        <Logo variant="icon-only" size="sm" />
                        <span className="font-title font-bold">TaskForce</span>
                    </Link>
                    <Link 
                        to="/auth" 
                        className="text-sm font-medium text-white-accent-dark hover:text-white-accent-light"
                    >
                        ← Retour
                    </Link>
                </div>

                <h1 className="mb-2 font-title text-3xl font-bold text-white-accent-light sm:text-4xl">
                    {title}
                </h1>
                <p className="mb-8 text-sm text-white-accent-dark">
                    Dernière mise à jour : {lastUpdated}
                </p>

                <div className="prose prose-invert prose-p:text-white-accent-dark prose-headings:text-white-accent-light prose-a:text-primary-default max-w-none">
                    {children}
                </div>
            </div>
        </div>
    );
};