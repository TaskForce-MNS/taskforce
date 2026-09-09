import { forwardRef, type ButtonHTMLAttributes } from 'react';
import { Spinner } from '../icons/index';

type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'danger' | 'success' | 'link' | 'passkey';
type ButtonSize = 'sm' | 'md' | 'lg';
type ButtonRadius = 'sm' | 'md' | 'lg' | 'full';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: ButtonVariant;
    size?: ButtonSize;
    radius?: ButtonRadius;
    isLoading?: boolean;
    leftIcon?: React.ReactNode;
    rightIcon?: React.ReactNode;
    fullWidth?: boolean;
}

const base = [
    'relative',
    'inline-flex items-center justify-center gap-2',
    'font-semibold font-text tracking-wide text-sm',
    'transition-all duration-200 ease-in-out',
    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
    'disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none disabled:hover:scale-100',
    'cursor-pointer select-none',
].join(' ');

const variants: Record<ButtonVariant, string> = {
    primary: [
        'bg-primary-default text-white-accent-light',
        'hover:shadow-[inset_0_-1px_0_0_theme(colors.primary.light)]',
        'hover:opacity-80',
        'focus-visible:ring-primary-default',
        'border border-transparent',
        'active:scale-[0.97]',
    ].join(' '),

    secondary: [
        'bg-secondary-dark text-white-accent-light',
        'border focus-visible:ring-secondary-default',
        'hover:opacity-80',
        'active:scale-[0.97]',
    ].join(' '),

    outline: [
        'bg-transparent text-white-accent-default',
        'border border-2 border-primary-dark',
        'hover:text-white-accent-light',
        'focus-visible:ring-primary-default',
        'active:scale-[0.97]',
    ].join(' '),

    danger: [
        'bg-error text-white-accent-light border',
        'hover:opacity-80',
        'focus-visible:ring-error',
        'active:scale-[0.97]',
    ].join(' '),
    success: [
        'bg-success text-white-accent-light border',
        'hover:opacity-80',
        'focus-visible:ring-success',
        'active:scale-[0.97]',
    ].join(' '),
    link: [
        'bg-transparent text-white-accent-dark',
        'underline underline-offset-4',
        'hover:text-white-accent-light',
        'focus-visible:ring-primary-default',
        'active:scale-[0.97]',
        '!p-0 !h-auto min-h-0 min-w-0'
    ].join(' '),
    passkey: [
        'group',
        'bg-white-accent-light text-black-accent-default',
        'shadow-[0_0_20px_rgba(255,255,255,0.15)]',
        'hover:bg-white-accent-default hover:scale-[1.01]',
        'focus-visible:ring-white-accent-light',
        'active:scale-[0.99]',
    ].join(' '),
};

const radiuses: Record<ButtonRadius, string> = {
    sm: 'rounded-small',
    md: 'rounded-medium',
    lg: 'rounded-large',
    full: 'rounded-full',
};

const sizes: Record<ButtonSize, string> = {
    sm: 'h-8  px-4  text-sm',
    md: 'h-10 px-6  text-base',
    lg: 'h-10 px-18  text-lg',
};


export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
    (
        {
            variant = 'primary',
            size = 'md',
            radius = 'sm',
            isLoading = false,
            leftIcon,
            rightIcon,
            fullWidth = false,
            disabled,
            children,
            type = 'button',
            className = '',
            ...props
        },
        ref
    ) => {
        const classes = [
            base,
            variants[variant],
            sizes[size],
            radiuses[radius],
            fullWidth ? 'w-full' : '',
            className,
        ]
            .filter(Boolean)
            .join(' ');


        return (
            <button
                ref={ref}
                type={type}
                disabled={disabled || isLoading}
                aria-busy={isLoading}
                className={classes}
                {...props}
            >
                <span className={`inline-flex items-center justify-center gap-2 transition-opacity duration-100 ${isLoading ? 'opacity-0' : 'opacity-100'}`}>
                    {leftIcon && <span aria-hidden="true">{leftIcon}</span>}
                    <span>{children}</span>
                    {rightIcon && <span aria-hidden="true">{rightIcon}</span>}
                </span>

                {isLoading && (
                    <span className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 flex items-center justify-center text-current">
                        <Spinner />
                    </span>
                )}
            </button>
        );
    }
);

Button.displayName = 'Button';