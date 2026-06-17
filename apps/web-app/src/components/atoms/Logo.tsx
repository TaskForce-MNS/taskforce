import { forwardRef, type HTMLAttributes } from 'react';

type LogoVariant = 'full' | 'icon-only' | 'text-only';
type LogoSize = 'sm' | 'md' | 'lg' | 'custom';
type LogoColor = 'default' | 'primary' | 'white' | 'black' | 'gradient' | 'currentColor';

interface LogoProps extends HTMLAttributes<HTMLDivElement> {
    variant?: LogoVariant;
    size?: LogoSize;
    colorTheme?: LogoColor;

    iconClassName?: string;
    textClassName?: string;
}

const SIZES: Record<Exclude<LogoSize, 'custom'>, { icon: string; text: string; gap: string }> = {
    sm: { icon: 'w-6  h-6', text: 'text-xl', gap: 'gap-2' },
    md: { icon: 'w-8  h-8', text: 'text-2xl', gap: 'gap-2' },
    lg: { icon: 'w-12 h-12', text: 'text-4xl', gap: 'gap-3' },
};

const COLOR_CLASSES: Record<LogoColor, string> = {
    default: 'text-white-accent-default',
    primary: 'text-primary-default',
    white: 'text-white',
    black: 'text-black',
    currentColor: '',
    gradient: '',
};

interface LogoIconProps {
    className?: string;
    isGradient: boolean;
}

const LogoIcon = ({ className, isGradient }: LogoIconProps) => (
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
);

export const Logo = forwardRef<HTMLDivElement, LogoProps>(
    (
        {
            variant = 'full',
            size = 'md',
            colorTheme = 'default',
            iconClassName,
            textClassName,
            className = 'font-logo ',
            ...props
        },
        ref
    ) => {
        const isGradient = colorTheme === 'gradient';
        const isCustom = size === 'custom';

        const resolved = isCustom ? null : SIZES[size];
        const iconClass = isCustom
            ? (iconClassName ?? 'w-8 h-8')
            : `${resolved!.icon} flex-shrink-0`;
        const textClass = isCustom
            ? (textClassName ?? 'text-2xl')
            : resolved!.text;
        const gapClass = isCustom ? 'gap-3' : resolved!.gap;

        if (variant === 'icon-only') {
            return (
                <div
                    ref={ref}
                    className={`inline-flex select-none ${COLOR_CLASSES[colorTheme]} ${className}`}
                    {...props}
                >
                    <LogoIcon className={iconClass} isGradient={isGradient} />
                </div>
            );
        }

       return (
            <div
                ref={ref}
                className={`inline-flex items-center select-none ${gapClass} ${COLOR_CLASSES[colorTheme]} ${className}`}
                {...props}
            >
                {variant !== 'text-only' && (
                    <LogoIcon className={iconClass} isGradient={isGradient} />
                )}

                <span
                    className={`font-extrabold tracking-tight ${textClass} ${isGradient
                        ? 'bg-gradient-to-br from-primary-default to-secondary-default bg-clip-text text-transparent'
                        : ''
                        } ${textClassName ?? ''}`}
                >
                    Taskforce
                </span>
            </div>
        );
    }
);

Logo.displayName = 'Logo';