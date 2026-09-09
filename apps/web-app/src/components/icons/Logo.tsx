interface LogoProps {
    className?: string;
    isGradient: boolean;
}
export const Logo = ({ className, isGradient }: LogoProps) => (
    <svg
        className={className}
        viewBox="0 0 202 203"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
    >
        {isGradient && (
            <defs>
                <linearGradient id="taskforce-gradient" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" className="text-primary-default" stopColor="currentColor" />
                    <stop offset="100%" className="text-secondary-default" stopColor="currentColor" />
                </linearGradient>
            </defs>
        )}
        <path
            fillRule="evenodd" clipRule="evenodd"
            d="M55.71 59.0706C44.36 44.3506 28.72 34.9004 7.20996 34.9004C20.16 52.0704 37.12 58.8706 55.71 59.0706Z"
            fill={isGradient ? 'url(#taskforce-gradient)' : 'currentColor'}
        />
        <path
            fillRule="evenodd" clipRule="evenodd"
            d="M145.7 59.0706C157.05 44.3506 172.69 34.9004 194.2 34.9004C181.25 52.0704 164.29 58.8706 145.7 59.0706Z"
            fill={isGradient ? 'url(#taskforce-gradient)' : 'currentColor'}
        />
        <path
            fillRule="evenodd" clipRule="evenodd"
            d="M100.71 37.5C105.23 30.83 129.51 0.959963 201.35 0.959963V20.3401C124.8 20.3401 112.02 136.4 110.03 202.24H91.3799C89.3899 136.4 76.6096 20.3401 0.0595703 20.3401V0.959963C71.8996 0.949963 96.19 30.83 100.71 37.5Z"
            fill={isGradient ? 'url(#taskforce-gradient)' : 'currentColor'}
        />
    </svg>
)