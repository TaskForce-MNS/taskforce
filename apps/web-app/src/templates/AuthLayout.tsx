import type { ReactNode } from 'react';

interface AuthLayoutProps {
    children: ReactNode;
}

export const AuthLayout = ({ children }: AuthLayoutProps) => {
    return (
        <div className="relative flex min-h-screen w-full items-center justify-center overflow-hidden p-m bg-gradient-to-br from-primary-dark to-secondary-dark" >
            <div className="absolute inset-0 z-0 flex items-center justify-center px-[10px]">

                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    className="h-auto w-full max-w-[600px] text-white-accent-light"
                    viewBox="0 0 202 203"
                    fill="none"
                >
                    <path
                        fillRule="evenodd"
                        clipRule="evenodd"
                        d="M55.71 59.0706C44.36 44.3506 28.72 34.9004 7.20996 34.9004C20.16 52.0704 37.12 58.8706 55.71 59.0706Z"
                        fill="currentColor"
                    />
                    <path
                        fillRule="evenodd"
                        clipRule="evenodd"
                        d="M145.7 59.0706C157.05 44.3506 172.69 34.9004 194.2 34.9004C181.25 52.0704 164.29 58.8706 145.7 59.0706Z"
                        fill="currentColor"
                    />
                    <path
                        fillRule="evenodd"
                        clipRule="evenodd"
                        d="M100.71 37.5C105.23 30.83 129.51 0.959963 201.35 0.959963V20.3401C124.8 20.3401 112.02 136.4 110.03 202.24H91.3799C89.3899 136.4 76.6096 20.3401 0.0595703 20.3401V0.959963C71.8996 0.949963 96.19 30.83 100.71 37.5Z"
                        fill="currentColor"
                    />
                </svg>
            </div>
            <div className="relative z-10 flex w-full max-w-lg flex-col items-center rounded-medium border border-white-accent-dark/20 bg-black-accent-dark/60 p-2xl shadow-xl backdrop-blur-xl">
                {children}
            </div>

        </div>
    )
}