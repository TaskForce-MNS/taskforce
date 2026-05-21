import { Input } from "../atoms/Input";

export const InputShowcase = () => {
    return (
        <section className="mb-12 rounded-medium border border-white-accent-dark/20 bg-black-accent-default p-2xl shadow-xl backdrop-blur-xl">
            <h2 className="text-xl font-semibold mb-6 border-b border-slate-800 pb-2 text-white-accent-default">
                2. Component: Input
            </h2>

            <div className="space-y-6 max-w-md">

                {/* Basic Test */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Standard</h3>
                    <Input placeholder="Enter your text..." />
                </div>

                {/* Complete Test (Label + Helper) */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">With Label & Helper</h3>
                    <Input
                        label="Email Address"
                        type="email"
                        placeholder="john.doe@company.com"
                        helperText="We will never share your email with anyone."
                    />
                </div>

                {/* Error Test (Passwordless friendly) */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Error State</h3>
                    <Input
                        label="Username"
                        type="text"
                        defaultValue="abc"
                        error="Username must contain at least 5 characters."
                    />
                </div>

                {/* Sizes Test */}
                <div className="space-y-4">
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Sizes</h3>
                    <Input
                        label="Small Input"
                        inputSize='sm'
                        placeholder="Small size input"
                    />
                    <Input
                        label="Medium Input"
                        inputSize='md'
                        placeholder="Medium size input"
                    />
                    <Input
                        label="Large Input"
                        inputSize='lg'
                        placeholder="Large size input"
                    />
                </div>

                {/* Icons Test */}
                <div className="space-y-4">
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">With Icons</h3>
                    <Input
                        label="Search"
                        placeholder="Search users..."
                        leftIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                                <path strokeLinecap="round" strokeLinejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
                            </svg>
                        }
                    />
                    <Input
                        label="Send Message"
                        placeholder="Type your message..."
                        rightIcon={
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor" className="size-6">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 0 1 .865-.501 48.172 48.172 0 0 0 3.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0 0 12 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018Z" />
                            </svg>
                        }
                    />

                    <Input
                        label="Amount"
                        type="number" min="0.00" step="any"
                        placeholder="0.00"
                        leftIcon={<span className="text-sm font-bold pl-1">€</span>}
                        rightIcon={<span className="text-xs font-bold pr-1">EUR</span>}
                    />
                </div>

                {/* Disabled Test */}
                <div>
                    <h3 className="text-sm text-white-accent-dark mb-3 uppercase tracking-wider">Disabled State</h3>
                    <Input
                        label="Disabled Field"
                        value="Not editable"
                        disabled
                    />
                </div>
            </div>
        </section>
    )
}