import { Textarea } from '@/components/atoms/Textarea';

export const TextareaShowcase = () => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                3. Component: Textarea
            </h2>

            <div className="space-y-6 max-w-md">

                {/* Basic Test */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Standard</h3>
                    <Textarea placeholder="Describe your project here..." />
                </div>

                {/* Helper Test */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">With Helper</h3>
                    <Textarea
                        placeholder="Describe your project here..."
                        helperText="This information will be displayed on your public profile."
                    />
                </div>

                {/* Error Test */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">With Label & Error</h3>
                    <Textarea
                        label="Biography"
                        placeholder="Tell us about yourself..."
                        error="Biography must be at least 100 characters long."
                        rows={3}
                    />
                </div>

                {/* Disabled Test */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Disabled State</h3>
                    <Textarea
                        label="Internal Notes"
                        placeholder="You don't have permission to edit this."
                        disabled
                        rows={2}
                    />
                </div>

                {/* Resizes Test */}
                <div className="space-y-4">
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Resize Options</h3>

                    <Textarea
                        label="Vertical Resize (Default)"
                        placeholder="I can only grow taller"
                        resize="vertical"
                        rows={2}
                    />

                    <Textarea
                        label="Horizontal Resize"
                        placeholder="I can only grow wider"
                        resize="horizontal"
                        rows={2}
                    />

                    <Textarea
                        label="Both Resize"
                        placeholder="I can grow in any direction"
                        resize="both"
                        rows={2}
                    />

                    <Textarea
                        label="Not Resizable"
                        placeholder="My size is strictly locked"
                        resize="none"
                        rows={2}
                    />
                </div>
            </div>
        </section>
    );
};