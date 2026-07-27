import { useState, type FormEvent } from 'react';
import { Input } from '@/components/atoms/Input';
import { Textarea } from '@/components/atoms/Textarea';
import { Button } from '@/components/atoms/Button';
import { useCreateProject } from '@/mutations/projects';

interface CreateProjectModalProps {
    isOpen: boolean;
    onClose: () => void;
}

const PRESET_COLORS = ['#587B7F', '#74394E', '#2D4953', '#470024', '#08B87D'];

export const CreateProjectModal = ({ isOpen, onClose }: CreateProjectModalProps) => {
    const [name, setName] = useState('');
    const [description, setDescription] = useState('');
    const [colorHex, setColorHex] = useState(PRESET_COLORS[0]);

    const { mutate, isPending } = useCreateProject();

    if (!isOpen) return null;

    const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        mutate(
            { name, description: description || undefined, colorHex },
            {
                onSuccess: () => {
                    setName('');
                    setDescription('');
                    setColorHex(PRESET_COLORS[0]);
                    onClose();
                },
            }
        );
    };

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black-accent-dark/80 p-4 backdrop-blur-sm"
            role="dialog"
            aria-modal="true"
            aria-labelledby="create-project-title"
            onClick={onClose}
        >
            <div
                className="w-full max-w-md rounded-medium border border-white-accent-dark/20
                           bg-black-accent-default p-6 shadow-xl"
                onClick={(e) => e.stopPropagation()}
            >
                <h2 id="create-project-title" className="mb-4 font-title text-xl font-bold text-white-accent-light">
                    Nouveau projet
                </h2>

                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    <Input
                        label="Nom du projet"
                        required
                        autoFocus
                        maxLength={100}
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="ex: Refonte du site web"
                    />

                    <Textarea
                        label="Description"
                        rows={3}
                        maxLength={500}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        placeholder="Optionnel"
                    />

                    <div className="flex flex-col gap-2">
                        <span className="text-sm font-medium text-white-accent-default">Couleur</span>
                        <div className="flex gap-2" role="radiogroup" aria-label="Couleur du projet">
                            {PRESET_COLORS.map((color) => (
                                <button
                                    key={color}
                                    type="button"
                                    role="radio"
                                    aria-checked={colorHex === color}
                                    aria-label={`Couleur ${color}`}
                                    onClick={() => setColorHex(color)}
                                    className={`h-8 w-8 rounded-full transition-transform
                                        ${colorHex === color ? 'scale-110 ring-2 ring-white-accent-light ring-offset-2 ring-offset-black-accent-default' : ''}`}
                                    style={{ backgroundColor: color }}
                                />
                            ))}
                        </div>
                    </div>

                    <div className="mt-2 flex justify-end gap-3">
                        <Button type="button" variant="ghost" onClick={onClose} disabled={isPending}>
                            Annuler
                        </Button>
                        <Button type="submit" variant="primary" isLoading={isPending}>
                            Créer le projet
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
};