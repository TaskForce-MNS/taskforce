import { forwardRef, type HTMLAttributes, type ReactNode } from 'react';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error';

export interface AlertProps extends HTMLAttributes<HTMLDivElement> {
    variant?: AlertVariant;
    title?: string;
    icon?: ReactNode;
    children: ReactNode;
}

const defaultIcons: Record<AlertVariant, ReactNode> = {
    info: (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" fill="currentColor" className="w-5 h-5">
            <path fillRule="evenodd" d="M18 10a8 8 0 1 1-16 0 8 8 0 0 1 16 0Zm-7-4a1 1 0 1 1-2 0 1 1 0 0 1 2 0ZM9 9a.75.75 0 0 0 0 1.5h.253a.25.25 0 0 1 .244.304l-.459 2.066A1.75 1.75 0 0 0 10.747 15H11a.75.75 0 0 0 0-1.5h-.253a.25.25 0 0 1-.244-.304l.459-2.066A1.75 1.75 0 0 0 9.253 9H9Z" clipRule="evenodd" />
        </svg>
    ),
    success: (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" fill="currentColor" className="w-5 h-5">
            <path fillRule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm3.857-9.809a.75.75 0 0 0-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 1 0-1.06 1.061l2.5 2.5a.75.75 0 0 0 1.137-.089l4-5.5Z" clipRule="evenodd" />
        </svg>
    ),
    warning: (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" fill="currentColor" className="w-5 h-5">
            <path fillRule="evenodd" d="M18 10a8 8 0 1 1-16 0 8 8 0 0 1 16 0Zm-8-5a.75.75 0 0 1 .75.75v4.5a.75.75 0 0 1-1.5 0v-4.5A.75.75 0 0 1 10 5Zm0 10a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z" clipRule="evenodd" />
        </svg>
    ),
    error: (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" fill="currentColor" className="w-5 h-5">
            <path fillRule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16ZM8.28 7.22a.75.75 0 0 0-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 1 0 1.06 1.06L10 11.06l1.72 1.72a.75.75 0 1 0 1.06-1.06L11.06 10l1.72-1.72a.75.75 0 0 0-1.06-1.06L10 8.94 8.28 7.22Z" clipRule="evenodd" />
        </svg>
    ),
};

export const Alert = forwardRef<HTMLDivElement, AlertProps>(
    (
        {
            variant = 'info',
            title,
            icon,
            children,
            className = '',
            ...props
        },
        ref
    ) => {

        const variantStyles: Record<AlertVariant, string> = {
            info: 'bg-info/10 border-blue-900/50 text-blue-200', //bg-blue-950/10
            success: 'bg-success/5 border-success/50 text-green-200', //bg-green-950/10
            warning: 'bg-warning/5 border-warning/50 text-yellow-200', //bg-yellow-950/10
            error: 'bg-error/5 border-error/50 text-red-200', //bg-red-950/30
        };

        const iconColors: Record<AlertVariant, string> = {
            info: 'text-blue-400',
            success: 'text-green-400',
            warning: 'text-yellow-400',
            error: 'text-red-400',
        };

        const ariaRole = variant === 'error' || variant === 'warning' ? 'alert' : 'status';

        return (
            <div
                ref={ref}
                role={ariaRole}
                className={`flex gap-3 rounded-small border p-4 ${variantStyles[variant]} ${className}`}
                {...props}
            >
                {/* Icône*/}
                <div className={`shrink-0 mt-0.5 ${iconColors[variant]}`}>
                    {icon || defaultIcons[variant]}
                </div>

                {/* Contenu */}
                <div className="flex flex-col gap-1">
                    {title && (
                        <h3 className="font-subtitle-md font-semibold tracking-wide">
                            {title}
                        </h3>
                    )}
                    <div className="text-sm opacity-90 leading-relaxed">
                        {children}
                    </div>
                </div>
            </div>
        );
    }
);

Alert.displayName = 'Alert';