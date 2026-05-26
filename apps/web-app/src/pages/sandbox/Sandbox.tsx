import { AlertShowcase } from '@/components/showcases/AlertShowcase';
import { ButtonShowcase } from '@/components/showcases/ButtonShowcase';
import { InputShowcase } from '@/components/showcases/InputShowcase';
import { LogoShowcase } from '@/components/showcases/LogoShowcase';
import { TestFluxShowcase } from '@/components/showcases/TestFluxShowcase';
import { TextareaShowcase } from '@/components/showcases/TextareaShowcase';
import { ToastShowcase } from '@/components/showcases/ToastShowcase';

export const Sandbox = () => {

    const title = '🧪 Bienvenue dans le laboratoire de composants !';

    const tableOfContents = [
        { id: 'buttons', label: 'Boutons' },
        { id: 'inputs', label: 'Inputs' },
        { id: 'textareas', label: 'Textareas' },
        { id: 'alerts', label: 'Alerts' },
        { id: 'toasts', label: 'Toasts' },
        { id: 'logos', label: 'Logos' },
        { id: 'testflux', label: 'TestFlux' },
    ]

    return (
        <div className="min-h-screen bg-black-accent-light text-white-accent-light flex flex-col md:flex-row">
            <aside className="w-fully md:w-64 p-m md:p-l border-b md:border-b-0 md:border-r border-slate-800 md:sticky md:top-0 md:h-screen md:overflow-y-auto shrink-0">
                <h2 className="text-xl font-title font-bold mb-m text-white-accent-default">Sommaire</h2>
                <nav className="flex flex-col space-y-s">
                    {tableOfContents.map((item) => (
                        <a
                            key={item.id}
                            href={`#${item.id}`}
                            className="text-slate-400 hover:text-color-primary-light transition-colors text-sm font-medium"
                        >
                            {item.label}
                        </a>
                    ))}
                </nav>
            </aside>
            <main className="flex-1 p-4 md:p-10 overflow-x-hidden">
                <div className="max-w-6xl mx-auto w-full">
                    <h1 className="text-3xl font-bold mb-2">{title}</h1>
                    <p className="text-slate-400 mb-xl">
                        Cette page sert uniquement au développement pour tester les UI isolées.
                    </p>
                    <div className="flex flex-col space-y-xl w-full">
                        <section id="buttons" className="scroll-mt-xl">
                            <ButtonShowcase label={tableOfContents[0].label} />
                        </section>
                        <section id="inputs" className="scroll-mt-xl">
                            <InputShowcase label={tableOfContents[1].label} />
                        </section>

                        <section id="textareas" className="scroll-mt-xl">
                            <TextareaShowcase label={tableOfContents[2].label} />
                        </section>

                        <section id="alerts" className="scroll-mt-xl">
                            <AlertShowcase label={tableOfContents[3].label} />
                        </section>

                        <section id="toasts" className="scroll-mt-xl">
                            <ToastShowcase label={tableOfContents[4].label} />
                        </section>

                        <section id="logos" className="scroll-mt-xl">
                            <LogoShowcase label={tableOfContents[5].label}/>
                        </section>

                        <section id="testflux" className="scroll-mt-xl">
                            <TestFluxShowcase />
                        </section>
                    </div>
                </div>
            </main>
        </div >
    );
}