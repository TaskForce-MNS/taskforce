import { Button } from '@/components/atoms/Button';

export const ButtonShowcase = () => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-dark p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                1. Composant : Button
            </h2>

            <div className="space-y-8">
                {/* Test des Couleurs (Variants) */}
                <div>
                    <h3 className="text-sm text-white-accent-dark  mb-3 uppercase tracking-wider">Variants</h3>
                    <div className="flex flex-wrap gap-4">
                        <Button variant="primary">Primary button</Button>
                        <Button variant="secondary">Secondary button</Button>
                        <Button variant="outline">Outline button</Button>
                        <Button variant="danger">Danger button</Button>
                        <Button variant="link">link button</Button>
                    </div>
                </div>

                {/* Test des Tailles */}
                <div>
                    <h3 className="text-sm text-white-accent-dark  mb-3 uppercase tracking-wider">Tailles (Sizes)</h3>
                    <div className="flex flex-wrap items-center gap-4">
                        <Button variant="outline" size="sm">Small Small Small</Button>
                        <Button variant="outline" size="md">Medium Medium Medium</Button>
                        <Button variant="outline" size="lg">Large Large Large Large</Button>
                    </div>
                </div>
                <div>
                    <h3 className="text-sm text-white-accent-dark  mb-3 uppercase tracking-wider">Largeur (width)</h3>
                    <div className="flex flex-wrap items-center gap-4">
                        <Button fullWidth>full Width</Button>
                        <Button className="w-100">Custom Width (w-100)</Button>
                    </div>
                </div>
                {/* Test des États (Loading / Disabled) */}
                <div>
                    <h3 className="text-sm text-white-accent-dark  mb-3 uppercase tracking-wider">États Spéciaux</h3>
                    <div className="flex flex-wrap gap-4">
                        <Button isLoading >Sauvegarde</Button>
                        <Button leftIcon={<svg xmlns="http://www.w3.org/2000/svg" fill="green" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-6">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                        </svg>

                        }>Check button</Button>
                        <Button leftIcon={<svg xmlns="http://www.w3.org/2000/svg" fill="red" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-6">
                            <path stroke-linecap="round" stroke-linejoin="round" d="m9.75 9.75 4.5 4.5m0-4.5-4.5 4.5M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                        </svg>

                        }>dissmiss button</Button>


                        <Button leftIcon={<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-6">
                            <path stroke-linecap="round" stroke-linejoin="round" d="m7.49 12-3.75 3.75m0 0 3.75 3.75m-3.75-3.75h16.5V4.499" />
                        </svg>
                        }>Button left icon</Button>
                        <Button rightIcon={<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-6">
                            <path stroke-linecap="round" stroke-linejoin="round" d="m16.49 12 3.75 3.75m0 0-3.75 3.75m3.75-3.75H3.74V4.499" />
                        </svg>

                        }>Button right icon</Button>

                    </div>
                </div>

            </div>
        </section>

    )
}