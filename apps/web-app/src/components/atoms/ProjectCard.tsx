import { Link } from '@tanstack/react-router';
import type { Project } from '@/api/project';

interface ProjectCardProps {
    project: Project;
}

export const ProjectCard = ({ project }: ProjectCardProps) => {
    const initial = project.name.charAt(0).toUpperCase();
    const color = project.colorHex ?? '#587B7F';

    return (
        <Link
            to="/projects/$projectId"
            params={{ projectId: project.id }}
            className="group flex flex-col gap-3 rounded-medium border border-white-accent-dark/10
                       bg-black-accent-default p-4 transition-all duration-200
                       hover:border-primary-default/50 hover:-translate-y-0.5"
        >
            <div className="flex items-center gap-3">
                {project.imageUrl ? (
                    <img
                        src={project.imageUrl}
                        alt=""
                        className="h-10 w-10 shrink-0 rounded-medium object-cover"
                    />
                ) : (
                    <div
                        className="flex h-10 w-10 shrink-0 items-center justify-center rounded-medium
                                   font-title font-bold text-white-accent-light"
                        style={{ backgroundColor: color }}
                        aria-hidden="true"
                    >
                        {initial}
                    </div>
                )}
                <h3 className="truncate font-title font-semibold text-white-accent-light
                               group-hover:text-primary-default transition-colors">
                    {project.name}
                </h3>
            </div>

            {project.description && (
                <p className="line-clamp-2 text-sm text-white-accent-dark">
                    {project.description}
                </p>
            )}
        </Link>
    );
};