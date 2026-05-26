import { Logo } from '@/components/atoms/Logo';

export const LogoShowcase = ({ label }: { label: string }) => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                6. {label}
            </h2>

            <div className="space-y-12">
                <div>
                    <h3 className="text-sm font-medium text-white-accent-dark/60 mb-4 uppercase tracking-wider">
                        Tailles (Sizes)
                    </h3>
                    <div className="flex flex-wrap items-end gap-8 border-b border-white-accent-dark/10 pb-8">
                        <Logo size="sm" />
                        <Logo size="md" />
                        <Logo size="lg" />
                    </div>
                </div>

                <div>
                    <h3 className="text-sm font-medium text-white-accent-dark/60 mb-4 uppercase tracking-wider">
                        Variantes (Variants)
                    </h3>
                    <div className="flex flex-wrap items-center gap-8 border-b border-white-accent-dark/10 pb-8">
                        <Logo variant="full" />
                        <Logo variant="icon-only" />
                        <Logo variant="text-only" />
                    </div>
                </div>

                <div>
                    <h3 className="text-sm font-medium text-white-accent-dark/60 mb-4 uppercase tracking-wider">
                        Thèmes (Colors)
                    </h3>
                    <div className="flex flex-wrap items-center gap-8">
                        <Logo colorTheme="white" />
                        <Logo colorTheme="black" />
                        <Logo colorTheme="primary" />

                        <Logo colorTheme="gradient" variant="icon-only" />
                        <Logo colorTheme="gradient" variant="text-only" />
                    </div>
                </div>
            </div>
        </section>
    );
};