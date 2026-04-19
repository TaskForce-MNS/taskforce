import { forwardRef, type ButtonHTMLAttributes } from 'react';

// ====================== TYPES ======================
type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'danger' | 'link';
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

// ====================== STYLES ======================
const base = [
    'inline-flex items-center justify-center gap-2',
    'font-semibold tracking-wide',
    'hover:opacity-80',
    'transition-all duration-200 ease-in-out',
    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
    'disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none',
    'cursor-pointer select-none',
].join(' ');

const variants: Record<ButtonVariant, string> = {
    primary: [
        'bg-primary-default text-white-accent-light',
        'hover:shadow-[inset_0_-1px_0_0_theme(colors.primary.light)]',
        'focus-visible:ring-primary-default',
        'border border-transparent',
        'active:scale-[0.97]',
    ].join(' '),

    secondary: [
        'bg-secondary-dark text-white-accent-light',
        'border focus-visible:ring-secondary-default',
        'active:scale-[0.97]',
    ].join(' '),

    outline: [
        'bg-transparent text-white-accent-light',
        'border border-2 border-primary-dark',
        'focus-visible:ring-primary-default',
        'active:scale-[0.97]',
    ].join(' '),

    danger: [
        'bg-error text-white-accent-light border',
        'hover:opacity-80',
        'focus-visible:ring-error',
        'active:scale-[0.97]',
    ].join(' '),
    link: [
        'bg-transparent text-white-accent-light', // Pas de fond, texte couleur principale
        'underline underline-offset-4',
        // Souligné avec un joli décalage de 4px (plus lisible)            // S'éclaircit (ou s'assombrit) au survol
        'focus-visible:ring-primary-default',
        'active:scale-[0.97]',

        '!p-0 !h-auto min-h-0 min-w-0'
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
    md: 'h-10 px-6  text-md',
    lg: 'h-10 px-18  text-lg',
};

// ====================== SPINNER ======================
const Spinner = () => (
    <svg
        className="animate-spin h-4 w-4"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        aria-hidden="true"
    >
        <circle
            className="opacity-25"
            cx="12" cy="12" r="10"
            stroke="currentColor"
            strokeWidth="4"
        />
        <path
            className="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
        />
    </svg>
);

// ====================== COMPONENT ======================
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
                disabled={disabled || isLoading}
                aria-busy={isLoading}
                className={classes}
                {...props}
            >
                {isLoading ? (
                    <>
                        <Spinner />
                        <span>{children}</span>
                    </>
                ) : (
                    <>
                        {leftIcon && <span aria-hidden="true">{leftIcon}</span>}
                        <span>{children}</span>
                        {rightIcon && <span aria-hidden="true">{rightIcon}</span>}
                    </>
                )}
            </button>
        );
    }
);

Button.displayName = 'Button';