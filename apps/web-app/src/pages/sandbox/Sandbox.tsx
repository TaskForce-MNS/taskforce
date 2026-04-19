import { Input } from '@/components/atoms/Inputs';
import { ButtonShowcase } from '@/components/showcases/ButtonShowcase';

export const Sandbox = () => {
    return (
        <div className="min-h-screen bg-white-accent-light p-10 text-white-light ">
            <div className="max-w-4xl mx-auto">
                <h1 className="text-3xl font-bold mb-2">Laboratoire de Composants 🧪</h1>
                <p className="text-slate-400 mb-10">
                    Cette page sert uniquement au développement pour tester les UI isolées.
                </p>

                {/* --- SECTION DES BOUTONS --- */}
                <ButtonShowcase />

                <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-dark p-2xl shadow-xl backdrop-blur-xl">
                    <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                        2. Composant : Input
                    </h2>

                    <div className="space-y-6 max-w-md">

                        {/* Test Basique */}
                        <div>
                            <h3 className="text-sm text-slate-400 mb-3 uppercase tracking-wider">Standard</h3>
                            <Input placeholder="Entrez votre texte..." />
                        </div>

                        {/* Test Complet (Label + Helper) */}
                        <div>
                            <h3 className="text-sm text-slate-400 mb-3 uppercase tracking-wider">Avec Label et Aide</h3>
                            <Input
                                label="Adresse Email"
                                type="email"
                                placeholder="jean.dupont@entreprise.com"
                                helperText="Nous ne partagerons jamais votre email."
                            />
                        </div>

                        {/* Test Erreur */}
                        <div>
                            <h3 className="text-sm text-slate-400 mb-3 uppercase tracking-wider">État d'Erreur</h3>
                            <Input
                                label="Mot de passe"
                                type="password"
                                defaultValue="123"
                                error="Le mot de passe doit contenir au moins 8 caractères."
                            />
                        </div>

                        {/* Test Icônes */}
                        <div>
                            <h3 className="text-sm text-slate-400 mb-3 uppercase tracking-wider">Avec Icônes</h3>
                            <Input
                                label="Recherche utilisateur"
                                placeholder="Rechercher par nom..."
                                leftIcon={
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
                                    </svg>
                                }
                            />
                        </div>
                        <div>
                            <h3 className="text-sm text-slate-400 mb-3 uppercase tracking-wider">Désactivé</h3>
                            <Input
                                label="Champ désactivé"
                                placeholder="Non modifiable"
                                value="Nom modifiable"
                                disabled
                            />
                        </div>
                    </div>
                </section>

            </div>
        </div>
    );
}