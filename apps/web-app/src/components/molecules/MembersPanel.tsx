import { useSuspenseQuery } from '@tanstack/react-query';
import { projectMembersQueryOptions } from '@/api/queries/projectsQueries';

const translateRole = (role: string): string => {
    switch (role.toLowerCase()) {
        case 'admin': return 'Administrateur';
        case 'owner': return 'Propriétaire';
        case 'member': return 'Membre';
        default: return role;
    }
};

const getRoleStyles = (role: string): string => {
    switch (role.toLowerCase()) {
        case 'owner':
            return 'border-primary-default/30 bg-primary-default/10 text-primary-light';
        case 'admin':
            return 'border-amber-400/20 bg-amber-400/10 text-amber-300';
        case 'member':
            return 'border-white-accent-dark/10 bg-white-accent-dark/5 text-white-accent-dark';
        default:
            return 'border-white-accent-dark/10 bg-white-accent-dark/5 text-white-accent-dark';
    }
};

const getInitials = (firstName?: string, lastName?: string): string => {
    const first = firstName?.charAt(0) || '';
    const last = lastName?.charAt(0) || '';
    const initials = `${first}${last}`.toUpperCase();
    return initials || '👤';
};

const formatJoinedAt = (date: string): string => {
    return new Date(date).toLocaleDateString('fr-FR', {
        day: 'numeric',
        month: 'short',
        year: 'numeric',
    });
};

const ROLE_WEIGHTS: Record<string, number> = { owner: 1, admin: 2, member: 3 };


export const MembersPanel = ({ projectId }: { projectId: string }) => {
    const { data: members } = useSuspenseQuery(projectMembersQueryOptions(projectId));

    const sortedMembers = [...members].sort((a, b) => {
        const weightA = ROLE_WEIGHTS[a.role.toLowerCase()] || 99;
        const weightB = ROLE_WEIGHTS[b.role.toLowerCase()] || 99;

        if (weightA !== weightB) return weightA - weightB;

        return (a.firstName || '').localeCompare(b.firstName || '');
    });

    return (
        <section className="overflow-hidden rounded-medium border border-white-accent-dark/10 bg-black-accent-default shadow-sm">
            {/* Header */}
            <div className="border-b border-white-accent-dark/10 px-5 py-4">
                <div className="flex items-center gap-3">
                    <h2 className="font-title text-lg font-semibold text-white-accent-light">
                        Membres de l’équipe
                    </h2>
                    <span className="inline-flex min-w-7 items-center justify-center rounded-full bg-white-accent-dark/10 px-2 py-0.5 text-xs font-semibold text-white-accent-dark">
                        {members.length}
                    </span>
                </div>
                <p className="mt-1 text-xs text-white-accent-dark">
                    Les personnes qui participent à cet espace de travail.
                </p>
            </div>

            {/* Membres */}
            <div className="p-3 sm:p-4">
                {sortedMembers.length === 0 ? (
                    <div className="rounded-small border border-dashed border-white-accent-dark/20 px-4 py-8 text-center">
                        <p className="text-sm font-medium text-white-accent-light">
                            Aucun membre
                        </p>
                        <p className="mt-1 text-xs text-white-accent-dark">
                            Les membres du projet apparaîtront ici.
                        </p>
                    </div>
                ) : (
                    <ul className="space-y-2">
                        {sortedMembers.map((member) => (
                            <li
                                key={member.userId}
                                className="group flex items-center justify-between gap-3 rounded-small border border-white-accent-dark/10 bg-black-accent-light/20 p-3 transition-colors hover:border-white-accent-dark/30 hover:bg-black-accent-light/40"
                            >
                                <div className="flex min-w-0 items-center gap-3">
                                    {/* Avatar */}
                                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-gradient-to-tr from-indigo-600 to-primary-default font-title text-xs font-bold text-white shadow-sm">
                                        {getInitials(member.firstName, member.lastName)}
                                    </div>

                                    {/* Infos */}
                                    <div className="min-w-0">
                                        <p className="truncate text-sm font-semibold text-white-accent-light">
                                            {member.firstName} {member.lastName}
                                        </p>
                                        <p className="mt-0.5 text-[11px] text-white-accent-dark">
                                            Membre depuis le {formatJoinedAt(member.joinedAt)}
                                        </p>
                                    </div>
                                </div>

                                {/* Role */}
                                <span
                                    className={`shrink-0 rounded-full border px-2.5 py-1 text-[10px] font-semibold uppercase tracking-wide ${getRoleStyles(member.role)}`}
                                >
                                    {translateRole(member.role)}
                                </span>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </section>
    );
};