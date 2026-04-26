import { AlertShowcase } from '@/components/showcases/AlertShowcase';
import { ButtonShowcase } from '@/components/showcases/ButtonShowcase';
import { InputShowcase } from '@/components/showcases/InputShowcase';
import { TestFluxShowcase } from '@/components/showcases/TestFluxShowcase';
import { TextareaShowcase } from '@/components/showcases/TextareaShowcase';

export const Sandbox = () => {
    return (
        <div className="min-h-screen bg-white-accent-light p-10 text-white-light ">
            <div className="max-w-4xl mx-auto">
                <h1 className="text-3xl font-bold mb-2">Laboratoire de Composants 🧪</h1>
                <p className="text-slate-400 mb-10">
                    Cette page sert uniquement au développement pour tester les UI isolées.
                </p>
                <ButtonShowcase />
                <InputShowcase />
                <TextareaShowcase />
                <AlertShowcase />
                <TestFluxShowcase />
            </div>
        </div>
    );
}