import { useSuspenseQuery } from '@tanstack/react-query';
import { projectQueryOptions } from '@/api/queries/projectsQueries';
import { Button } from '@/components/atoms/Button';
import { MembersPanel } from '@/components/molecules/MembersPanel';
import { useCallback, useState, useRef } from 'react';
import { EditProjectModal } from '@/pages/ProjectDetail/EditProjectModal';
import { CalendarTimeline, type CalendarTimelineHandle } from '@/components/molecules/CalendarTimeline';

export const ProjectDetail = ({ projectId }: { projectId: string }) => {
    const { data: project } = useSuspenseQuery(projectQueryOptions(projectId));
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);

    const calendarRef = useRef<CalendarTimelineHandle>(null);

    const canManage = project.currentUserRole === 'Owner';
    console.log(project.currentUserRole);
    const [headerDateTitle, setHeaderDateTitle] = useState<string>('Chargement...');

    const handleDateChange = useCallback((newDateTitle: string) => {
        setHeaderDateTitle(newDateTitle);
    }, []);
    const handleGoToToday = () => {
        calendarRef.current?.scrollToToday();
        console.log(calendarRef.current);
    }
    return (

        <div className="flex h-full w-full flex-col space-y-4 sm:space-y-6">

            <div className="flex shrink-0 flex-col items-start gap-4 sm:flex-row sm:items-center sm:justify-between">

                <div className="flex min-w-0 items-center gap-4">
                    {/* <div
                        className="flex h-10 w-10 sm:h-12 sm:w-12 shrink-0 items-center justify-center rounded-medium font-title text-lg sm:text-xl font-bold text-white-accent-light shadow-sm"
                        style={{ backgroundColor: project.colorHex ?? '#587B7F' }}
                    >
                        {project.name.charAt(0).toUpperCase()}
                    </div> */}
                    <div className="min-w-0">
                        <h1
                            onClick={handleGoToToday}
                            title="Revenir à aujourd'hui"
                            className="truncate font-title text-xl sm:text-2xl font-bold text-white-accent-light transition-colors duration-300 cursor-pointer hover:text-primary-light"
                        >
                            {headerDateTitle}
                        </h1>
                    </div>
                </div>

                <div className="flex shrink-0 items-center gap-3">
                    <MembersPanel projectId={projectId} />

                    {canManage && (
                        <Button
                            variant="outline"
                            onClick={() => setIsEditModalOpen(true)}
                            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full border border-white-accent-dark/15 bg-black-accent-light/30 p-0 text-white-accent-dark transition-colors hover:border-white-accent-dark/30 hover:bg-white-accent-dark/10 hover:text-white-accent-light shadow-sm"
                            title="Paramètres du projet"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.8} stroke="currentColor" className="h-4 w-4">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M10.343 3.94c.09-.542.56-.94 1.11-.94h1.093c.55 0 1.02.398 1.11.94l.149.894c.07.424.384.764.78.93.398.164.855.142 1.205-.108l.737-.527a1.125 1.125 0 0 1 1.45.12l.773.774c.39.389.44 1.002.12 1.45l-.527.737c-.25.35-.272.806-.107 1.204.165.397.505.71.93.78l.893.15c.543.09.94.56.94 1.109v1.094c0 .55-.397 1.02-.94 1.11l-.893.149c-.425.07-.765.383-.93.78-.165.398-.143.854.107 1.204l.527.738c.32.447.269 1.06-.12 1.45l-.774.773a1.125 1.125 0 0 1-1.449.12l-.738-.527c-.35-.25-.806-.272-1.203-.107-.397.165-.71.505-.781.929l-.149.894c-.09.542-.56.94-1.11.94h-1.094c-.55 0-1.019-.398-1.11-.94l-.148-.894c-.071-.424-.384-.764-.781-.93-.398-.164-.854-.142-1.204.108l-.738.527c-.447.32-1.06.269-1.45-.12l-.773-.774a1.125 1.125 0 0 1-.12-1.45l.527-.737c.25-.35.273-.806.108-1.204-.165-.397-.505-.71-.93-.78l-.894-.15c-.542-.09-.94-.56-.94-1.109v-1.094c0-.55.398-1.02.94-1.11l.894-.149c.424-.07.765-.383.93-.78.165-.398.143-.854-.107-1.204l-.527-.738a1.125 1.125 0 0 1 .12-1.45l.773-.773a1.125 1.125 0 0 1 1.45-.12l.737.527c.35.25.807.272 1.204.107.397-.165.71-.505.78-.929l.15-.894Z" />
                                <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
                            </svg>
                        </Button>
                    )}
                </div>
            </div>

            <CalendarTimeline projectId={projectId} ref={calendarRef} key={projectId} onDateChange={handleDateChange} />

            {isEditModalOpen && (
                <EditProjectModal
                    project={project}
                    onClose={() => setIsEditModalOpen(false)}

                    updatedDate={new Date(project.updatedAt).toLocaleDateString('fr-FR')}
                />
            )}
        </div>
    );
};