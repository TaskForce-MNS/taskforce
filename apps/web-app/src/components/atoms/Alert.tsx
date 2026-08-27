import { forwardRef, type HTMLAttributes, type ReactNode } from 'react';
import { Info, Success, Warning, Error } from '../icons/index';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error';

export interface AlertProps extends HTMLAttributes<HTMLDivElement> {
    variant?: AlertVariant;
    title?: string;
    icon?: ReactNode;
    children: ReactNode;
}

const defaultIcons: Record<AlertVariant, ReactNode> = {
    info: (
        <Info />
    ),
    success: (
        <Success />
    ),
    warning: (
        <Warning />
    ),
    error: (
        <Error />
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