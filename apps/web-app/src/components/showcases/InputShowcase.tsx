import { Input } from "../atoms/Inputs";

export const InputShowcase = () => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-dark p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                2. Composant : Input
            </h2>

            <div className="space-y-6 max-w-md">

                {/* Test Basique */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Standard</h3>
                    <Input placeholder="Entrez votre texte..." />
                </div>

                {/* Test Complet (Label + Helper) */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Avec Label et Aide</h3>
                    <Input
                        label="Adresse Email"
                        type="email"
                        placeholder="jean.dupont@entreprise.com"
                        helperText="Nous ne partagerons jamais votre email."
                    />
                </div>

                {/* Test Erreur */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">État d'Erreur</h3>
                    <Input
                        label="Mot de passe"
                        type="password"
                        defaultValue="123"
                        error="Le mot de passe doit contenir au moins 8 caractères."
                    />
                </div>
                {/* Test Sizes */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Tailles</h3>
                    <Input
                        label="small input"
                        inputSize='sm'
                        placeholder="Small size input"
                    />
                    <Input
                        label="medium input"
                        inputSize='md'
                        placeholder="Medium size input"
                    />
                    <Input
                        label="large input"
                        inputSize='lg'
                        placeholder="Large size input"
                    />

                </div>

                {/* Test Icônes */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Avec Icônes</h3>
                    <Input
                        label="Recherche utilisateur"
                        placeholder="Left icon input"
                        leftIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
                            </svg>
                        }
                    />
                    <Input
                        label="Envoyer un message"
                        placeholder="Right icon input"
                        rightIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-6">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 0 1 .865-.501 48.172 48.172 0 0 0 3.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0 0 12 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018Z" />
                            </svg>


                        }
                    />
                </div>
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Désactivé</h3>
                    <Input
                        label="Champ désactivé"
                        value="Non modifiable"
                        disabled
                    />
                </div>
            </div>
        </section>
    )
} 