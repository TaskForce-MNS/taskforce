import { useState } from 'react';
import { useAuthStore, ExperienceLevel } from '@/stores/useAuthStore';
import { UserSkills } from '@/mutations/UserSkills';

export const SkillsManager = () => {
    const user = useAuthStore((state) => state.user);
    const { addSkill, removeSkill } = UserSkills();

    const [domain, setDomain] = useState('');
    const [level, setLevel] = useState<ExperienceLevel>(ExperienceLevel.Intermediate);

    if (!user) return null;

    const handleAdd = (e: React.FormEvent) => {
        e.preventDefault();
        if (!domain.trim()) return;
        addSkill.mutate({ domain: domain.trim(), level });
        setDomain(''); // Reset le champ
    };

    const getLevelLabel = (lvl: ExperienceLevel) => {
        switch (lvl) {
            case ExperienceLevel.Junior: return 'Junior';
            case ExperienceLevel.Intermediate: return 'Intermédiaire';
            case ExperienceLevel.Senior: return 'Senior';
            default: return 'Inconnu';
        }
    };

    return (
        <div className="flex flex-col gap-6 p-4 text-white-accent-light">
            <div>
                <h3 className="font-title text-lg font-bold text-white">Mes Compétences</h3>
                <p className="text-sm text-white-accent-dark">
                    Ajoutez vos technologies pour recevoir des tâches adaptées à votre profil.
                </p>
            </div>

            {/* Formulaire d'ajout */}
            <form onSubmit={handleAdd} className="flex flex-col gap-3 sm:flex-row sm:items-end">
                <div className="flex flex-1 flex-col gap-1">
                    <label className="text-xs font-bold text-white-accent-dark">Technologie (ex: React, C#)</label>
                    <input
                        type="text"
                        value={domain}
                        onChange={(e) => setDomain(e.target.value)}
                        className="rounded-lg border border-white-accent-dark/15 bg-black-accent-light/50 p-2 text-sm text-white outline-none focus:border-primary-default"
                        placeholder="Ex: TypeScript"
                    />
                </div>

                <div className="flex flex-col gap-1">
                    <label className="text-xs font-bold text-white-accent-dark">Niveau</label>
                    <select
                        value={level}
                        onChange={(e) => setLevel(Number(e.target.value) as ExperienceLevel)}
                        className="rounded-lg border border-white-accent-dark/15 bg-black-accent-light/50 p-2 text-sm text-white outline-none focus:border-primary-default"
                    >
                        <option value={ExperienceLevel.Junior}>Junior</option>
                        <option value={ExperienceLevel.Intermediate}>Intermédiaire</option>
                        <option value={ExperienceLevel.Senior}>Senior</option>
                    </select>
                </div>

                <button
                    type="submit"
                    disabled={addSkill.isPending || !domain.trim()}
                    className="rounded-lg bg-primary-default px-4 py-2 text-sm font-bold text-white transition-colors hover:bg-indigo-600 disabled:opacity-50"
                >
                    {addSkill.isPending ? '...' : 'Ajouter'}
                </button>
            </form>

            {/* Liste des compétences actuelles */}
            <div className="flex flex-wrap gap-2">
                {user.skills?.length === 0 && (
                    <span className="text-sm italic text-white-accent-dark">Aucune compétence déclarée.</span>
                )}
                {user.skills?.map((skill) => (
                    <div
                        key={skill.id}
                        className="flex items-center gap-2 rounded-lg border border-white-accent-dark/15 bg-black-accent-light/30 py-1 pl-3 pr-1 text-sm transition-colors hover:border-white-accent-dark/30"
                    >
                        <span className="font-bold text-white">{skill.domain}</span>
                        <span className="text-xs text-white-accent-dark">({getLevelLabel(skill.level)})</span>
                        <button
                            onClick={() => removeSkill.mutate(skill.id)}
                            disabled={removeSkill.isPending}
                            className="ml-1 flex h-6 w-6 items-center justify-center rounded-md text-red-400 hover:bg-red-400/10 hover:text-red-300"
                            title="Retirer"
                        >
                            ✕
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
};