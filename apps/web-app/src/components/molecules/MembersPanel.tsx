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

// const getRoleStyles = (role: string): string => {
//     switch (role.toLowerCase()) {
//         case 'owner':
//             return 'border-primary-default/30 bg-primary-default/10 text-primary-light';
//         case 'admin':
//             return 'border-amber-400/20 bg-amber-400/10 text-amber-300';
//         case 'member':
//             return 'border-white-accent-dark/10 bg-white-accent-dark/5 text-white-accent-dark';
//         default:
//             return 'border-white-accent-dark/10 bg-white-accent-dark/5 text-white-accent-dark';
//     }
// };

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
        <div className="flex max-w-[280px] sm:max-w-[350px] items-center gap-1 overflow-x-auto rounded-full border border-white-accent-dark/15 bg-black-accent-light/40 p-1.5 backdrop-blur-md shadow-sm scrollbar-hide">

            {sortedMembers.length === 0 ? (
                <span className="px-3 py-1 text-xs text-white-accent-dark">Aucun membre</span>
            ) : (
                sortedMembers.map((member) => (
                    <div
                        key={member.userId}
                        className="group flex shrink-0 cursor-pointer items-center gap-2 rounded-xl px-2 py-1 transition-colors hover:bg-white-accent-dark/10"
                        title={`${translateRole(member.role)} - Rejoint le ${formatJoinedAt(member.joinedAt)}`}
                    >
                        <div className="flex h-6 w-6 shrink-0 items-center justify-center rounded-xl bg-gradient-to-tr from-indigo-600 to-primary-default font-title text-[9px] font-bold text-white shadow-sm">
                            {getInitials(member.firstName, member.lastName)}
                        </div>

                        <span className="text-xs font-medium text-white-accent-light">
                            {member.firstName}
                        </span>
                    </div>
                ))
            )}
        </div>
    );
};