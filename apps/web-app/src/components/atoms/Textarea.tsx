import { forwardRef, type TextareaHTMLAttributes, useId } from 'react';

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
    label?: string;
    error?: string;
    helperText?: string;
    fullWidth?: boolean;
    rows?: number;
    resize?: 'none' | 'vertical' | 'horizontal' | 'both';
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
    (
        {
            label,
            error,
            helperText,
            fullWidth = true,
            rows = 4,
            resize = 'vertical',
            className = '',
            id,
            disabled,
            ...props
        },
        ref
    ) => {
        const generatedId = useId();
        const textareaId = id || generatedId;

        const resizeClasses = {
            none: 'resize-none',
            vertical: 'resize-y',
            horizontal: 'resize-x',
            both: 'resize',
        };

        const baseStyles = [
            'w-full rounded-large border bg-transparent text-white-accent-light placeholder-white-accent-dark p-3',
            'transition-all duration-200 ease-in-out',
            'focus:outline-none focus:ring-2 focus:ring-offset-0',
            'disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-black-accent-light',
            resizeClasses[resize],
            error
                ? 'border-error focus:border-error focus:ring-error/20'
                : 'border-slate-700 hover:border-primary-dark focus:border-primary-default focus:ring-primary-default/20',
            className,
        ].filter(Boolean).join(' ');

        return (
            <div className={`flex flex-col gap-1.5 ${fullWidth ? 'w-full' : ''}`}>
                {label && (
                    <label
                        htmlFor={textareaId}
                        className="text-sm font-medium text-white-accent-default select-none"
                    >
                        {label}
                    </label>
                )}

                <textarea
                    ref={ref}
                    id={textareaId}
                    rows={rows}
                    disabled={disabled}
                    aria-invalid={!!error}
                    aria-required={props.required}
                    aria-describedby={
                        error ? `${textareaId}-error` : helperText ? `${textareaId}-helper` : undefined
                    }
                    className={baseStyles}
                    {...props}
                />

                {error ? (
                    <span id={`${textareaId}-error`} role="alert" className="text-sm text-error">
                        {error}
                    </span>
                ) : helperText ? (
                    <span id={`${textareaId}-helper`} className="text-sm text-white-accent-default/80">
                        {helperText}
                    </span>
                ) : null}
            </div>
        );
    }
);

Textarea.displayName = 'Textarea';