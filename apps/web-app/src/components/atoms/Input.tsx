import { forwardRef, type InputHTMLAttributes, type ReactNode, useId } from 'react';

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
    label?: string;
    error?: string;
    helperText?: string;
    leftIcon?: ReactNode;
    rightIcon?: ReactNode;
    fullWidth?: boolean;
    inputSize?: 'sm' | 'md' | 'lg';
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
    (
        {
            label,
            error,
            helperText,
            leftIcon,
            rightIcon,
            fullWidth = true,
            inputSize = 'md',
            className = '',
            id,
            disabled,
            ...props
        },
        ref
    ) => {
        const generatedId = useId();
        const inputId = id || generatedId;

        const sizes = {
            sm: 'h-8 text-sm',
            md: 'h-10 text-sm',
            lg: 'h-12 text-base',
        };

        const baseInputStyles = [
            ' w-full rounded-large border bg-black-accent-light text-white-accent-light placeholder-white-accent-dark',
            'transition-all duration-200 ease-in-out',
            'focus:outline-none focus:ring-2 focus:ring-offset-0',
            'disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-black-accent-light',
            sizes[inputSize],
            error
                ? 'border-error focus:border-error focus:ring-error/20'
                : 'border-slate-700 hover:border-primary-dark focus:border-primary-default focus:ring-primary-default/20',
            leftIcon ? 'pl-10' : 'pl-3',
            rightIcon ? 'pr-10' : 'pr-3',
            className,
        ].filter(Boolean).join(' ');

        let ariaDescribedBy: string | undefined = undefined;
        if (error) {
            ariaDescribedBy = `${inputId}-error`;
        } else if (helperText) {
            ariaDescribedBy = `${inputId}-helper`;
        }

        let helperOrErrorText = null;
        if (error) {
            helperOrErrorText = (
                <span id={`${inputId}-error`} role="alert" className="text-sm text-error">
                    {error}
                </span>
            );
        } else if (helperText) {
            helperOrErrorText = (
                <span id={`${inputId}-helper`} className="text-sm text-white-accent-default/80">
                    {helperText}
                </span>
            );
        }

        return (
            <div className={`flex flex-col gap-1.5 ${fullWidth ? 'w-full' : ''}`}>
                {label && (
                    <label
                        htmlFor={inputId}
                        className="text-sm font-medium text-white-accent-default select-none"
                    >
                        {label}
                    </label>
                )}

                <div className="relative flex items-center group">
                    {leftIcon && (
                        <span className="absolute left-3 text-white-accent-default pointer-events-none transition-colors group-focus-within:text-primary-default">
                            {leftIcon}
                        </span>
                    )}

                    <input
                        ref={ref}
                        id={inputId}
                        disabled={disabled}
                        aria-invalid={!!error}
                        aria-required={props.required}
                        aria-describedby={ariaDescribedBy}
                        className={baseInputStyles}
                        {...props}
                    />

                    {rightIcon && (
                        <span className="absolute right-3 text-white-accent-light pointer-events-none transition-colors group-focus-within:text-primary-default">
                            {rightIcon}
                        </span>
                    )}
                </div>

                {helperOrErrorText}
            </div>
        );
    }
);

Input.displayName = 'Input';