import { forwardRef, type HTMLAttributes } from 'react';
import { Logo as LogoIcon } from '../icons/index';

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