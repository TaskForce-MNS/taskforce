import { useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi, type CreateTaskPayload, type UpdateTaskPayload, type Task } from '@/api/task';
import { useToastStore } from '@/stores/useToastStore';

export const useCreateTask = (projectId: string) => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: (payload: CreateTaskPayload) => tasksApi.create(payload),
        onSuccess: (newTask) => {
            queryClient.setQueryData<Task[]>(
                ['projects', projectId, 'tasks'],
                (old = []) => [...old, newTask]
            );

            addToast({
                variant: 'success',
                title: 'Tâche créée',
                message: 'La tâche a été ajoutée au projet.',
            });
        },
        onError: (error) => {
            addToast({
                variant: 'error',
                title: 'Erreur',
                message: error instanceof Error ? error.message : 'Impossible de créer la tâche.'
            });
        },
    });
};

export const useUpdateTask = (projectId: string) => {
    const queryClient = useQueryClient();
    const addToast = useToastStore((state) => state.addToast);

    return useMutation({
        mutationFn: ({ taskId, payload }: { taskId: string; payload: UpdateTaskPayload }) =>
            tasksApi.update(taskId, payload),
        onSuccess: (updatedTask) => {
            queryClient.setQueryData<Task[]>(
                ['projects', projectId, 'tasks'],
                (old = []) => old.map((task) => (task.id === updatedTask.id ? updatedTask : task))
            );
        },
        onError: (error) => {
            addToast({
                variant: 'error',
                title: 'Erreur',
                message: error instanceof Error ? error.message : 'Impossible de modifier la tâche.'
            });
        },
    });
};