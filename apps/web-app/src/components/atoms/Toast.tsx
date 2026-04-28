import { useToastStore, type ToastVariant, type Toast } from '@/stores/useToastStore';
import type { JSX } from 'react';

const ToastIcons: Record<ToastVariant, JSX.Element> = {
    success: (
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 22 22" strokeWidth={1.5} stroke="currentColor" className="size-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
        </svg>
    ),
    error: (
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 22 22" strokeWidth={1.5} stroke="currentColor" className="size-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m9-.75a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 3.75h.008v.008H12v-.008Z" />
        </svg>
    ),
    info: (
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 22 22" strokeWidth={1.5} stroke="currentColor" className="size-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="m11.25 11.25.041-.02a.75.75 0 0 1 1.063.852l-.708 2.836a.75.75 0 0 0 1.063.853l.041-.021M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9-3.75h.008v.008H12V8.25Z" />
        </svg>
    ),
    warning: (
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 22 22" strokeWidth={1.5} stroke="currentColor" className="size-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z" />
        </svg>
    ),
};

const toastStyles: Record<ToastVariant, string> = {
    success: 'border-success/20 bg-success/5',
    error: 'border-error/20 bg-error/5',
    info: 'border-info/20 bg-info/5',
    warning: 'border-warning/20 bg-warning/5',
};

const ToastItem = ({ toast, onRemove }: { toast: Toast; onRemove: (id: string) => void }) => (
    <div
        role={toast.variant === 'error' || toast.variant === 'warning' ? 'alert' : 'status'}
        className={`
            pointer-events-auto w-full max-w-sm sm:max-w-md
            flex items-start gap-3 overflow-hidden
            rounded-medium border p-4 shadow-lg backdrop-blur-xl text-white-accent-light
            transform transition-all duration-300 ease-out
            animate-in slide-in-from-right-full fade-in
            ${toastStyles[toast.variant]}
        `}
    >
        {/* Icône */}
        <div className="flex-shrink-0 mt-0.5" aria-hidden="true">
            {ToastIcons[toast.variant]}
        </div>

        {/* Contenu */}
        <div className="flex-1 min-w-0">
            <p className="text-md font-semibold text-white-accent-light">
                {toast.title}
            </p>
            {toast.message && (
                <p className="mt-1 text-sm text-white-accent-default break-words">
                    {toast.message}
                </p>
            )}
        </div>

        {/* Bouton fermer */}
        <button
            onClick={() => onRemove(toast.id)}
            className="flex-shrink-0 ml-4 rounded-small opacity-50 hover:opacity-100 transition-opacity focus:outline-none focus:ring-2 focus:ring-white-accent-dark"
            aria-label="Fermer la notification"
        >
            <span aria-hidden="true" className="text-white-accent-light text-lg leading-none"><svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="size-6">
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
            </span>
        </button>
    </div>
);

export const ToastContainer = () => {
    const { toasts, removeToast } = useToastStore();

    const baseClass = "fixed bottom-0 right-0 z-50 flex flex-col items-end gap-3 p-4 sm:p-6 pointer-events-none";

    return (
        <div aria-live="polite" aria-atomic="true" className={baseClass}>
            {toasts.map((toast) => (
                <ToastItem
                    key={toast.id}
                    toast={toast}
                    onRemove={removeToast}
                />
            ))}
        </div>
    );
};