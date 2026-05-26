import { Alert } from '@/components/atoms/Alert';

export const AlertShowcase = ({ label }: { label: string }) => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                4. {label}
            </h2>

            <div className="space-y-6 max-w-md">

                {/* Test des Variants (Avec Titre) */}
                <div className="space-y-4">
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Standard Variants (With Title)</h3>

                    <Alert variant="info" title="New Update Available">
                        A new version of the application is ready. Please refresh the page to apply the latest security patches.
                    </Alert>

                    <Alert variant="success" title="Passkey Registered">
                        Your device has been successfully registered. You can now use it to sign in securely without a password.
                    </Alert>

                    <Alert variant="warning" title="Incomplete Profile">
                        Your profile is missing some important information. Please complete your setup to access all features.
                    </Alert>

                    <Alert variant="error" title="Authentication Failed">
                        The RP ID "taskforce.local" is invalid for this domain. Please ensure you are initiating the request from the correct origin.
                    </Alert>
                </div>

                {/* Test sans Titre (Format Simple) */}
                <div className="space-y-4">
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Minimalist (No Title)</h3>

                    <Alert variant="info">
                        Just a quick note: maintenance is scheduled for tonight at 2 AM.
                    </Alert>

                    <Alert variant="error">
                        Connection lost. We are trying to reconnect you...
                    </Alert>
                </div>

            </div>
        </section>
    );
};