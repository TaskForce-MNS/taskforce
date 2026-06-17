import { forwardRef, type ReactNode } from 'react';

export type WorkspaceButtonVariant = 'project' | 'action-success' | 'home';

interface WorkspaceButtonProps {
    children: ReactNode;
    onClick?: () => void;
    isActive?: boolean;
    variant?: WorkspaceButtonVariant;
    showIndicator?: boolean;
    label: string;
    disabled?: boolean;
}

export const WorkspaceButton = forwardRef<HTMLButtonElement, WorkspaceButtonProps>(
    ({ children, onClick, isActive = false, variant = 'project', showIndicator = true, label, disabled }, ref) => {

        const getVariantClasses = () => {
            switch (variant) {
                case 'home':
                    return 'bg-primary-light text-white-accent-light';
                case 'action-success':
                    return 'bg-black-accent-light text-success hover:bg-success hover:text-white-accent-light';
                case 'project':
                default:
                    return 'bg-black-accent-light text-white-accent-light hover:bg-primary-default';
            }
        };

        return (
            <div className="group relative flex w-full items-center justify-center">
                {showIndicator && (
                    <div className={`absolute left-0 w-1.5 bg-white-accent-default rounded-r-full transition-all duration-200
                        ${isActive ? 'h-1   opacity-100' : 'h-5 opacity-0 group-hover:opacity-100'}`}
                    />
                )}
                <button
                    ref={ref}
                    type="button"
                    onClick={onClick}
                    disabled={disabled}
                    aria-pressed={isActive}
                    aria-label={label}
                    className={`flex h-12 w-12 cursor-pointer items-center justify-center transition-all duration-200 font-text 
                        disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none
                        ${getVariantClasses()}
                        ${isActive ? 'rounded-[16px] bg-primary-default font-bold' : 'rounded-[20px] group-hover:rounded-[16px]'}`}
                >
                    {children}
                </button>
            </div>
        );
    }
);

WorkspaceButton.displayName = 'WorkspaceButton';