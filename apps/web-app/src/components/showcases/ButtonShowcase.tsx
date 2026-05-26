import { Button } from '@/components/atoms/Button';

export const ButtonShowcase = ({ label }: { label: string }) => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                1. {label}
            </h2>

            <div className="space-y-8">
                {/* Test des Variants */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Variants</h3>
                    <div className="flex flex-wrap gap-4">
                        <Button variant="primary">Primary Button</Button>
                        <Button variant="secondary">Secondary Button</Button>
                        <Button variant="outline">Outline Button</Button>
                        <Button variant="danger">Danger Button</Button>
                        <Button variant="link">Link Button</Button>
                    </div>
                </div>

                {/* Test des Tailles */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Sizes</h3>
                    <div className="flex flex-wrap items-center gap-4">
                        <Button variant="outline" size="sm">Small</Button>
                        <Button variant="outline" size="md">Medium</Button>
                        <Button variant="outline" size="lg">Large</Button>
                    </div>
                </div>

                {/* Test des Largeurs */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Widths</h3>
                    <div className="flex flex-wrap items-center gap-4">
                        <Button fullWidth>Full Width</Button>
                        {/* Remarque : w-100 n'existe pas dans Tailwind par défaut. C'est w-96 ou w-full. J'ai mis w-64 pour l'exemple */}
                        <Button className="w-64">Custom Width (w-64)</Button>
                    </div>
                </div>

                {/* Test des États */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Special States</h3>
                    <div className="flex flex-wrap gap-4">
                        <Button isLoading>Saving...</Button>
                        <Button leftIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                            </svg>
                        }>Confirm</Button>
                        <Button variant="danger" leftIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="m9.75 9.75 4.5 4.5m0-4.5-4.5 4.5M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                            </svg>
                        }>Dismiss</Button>

                        <Button leftIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="m7.49 12-3.75 3.75m0 0 3.75 3.75m-3.75-3.75h16.5V4.499" />
                            </svg>
                        }>Left Icon</Button>
                        <Button rightIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="m16.49 12 3.75 3.75m0 0-3.75 3.75m3.75-3.75H3.74V4.499" />
                            </svg>
                        }>Right Icon</Button>

                        {/* J'ai ajouté l'état Disabled que tu as oublié dans le showcase ! */}
                        <Button disabled>Disabled</Button>
                    </div>
                </div>

            </div>
        </section>
    );
};