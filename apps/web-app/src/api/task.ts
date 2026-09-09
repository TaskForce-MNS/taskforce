import { apiClient } from '@/api/client';
import { ListProjects, TasksBase, CreateTask } from '@/api/config';

export interface Task {
    id: string;
    name: string;
    description: string | null;
    isChecked: boolean;
    isArchived: boolean;
    createdAt: string;
    projectId: string;
}

export interface CreateTaskPayload {
    name: string;
    description?: string | null;
    projectId: string;
}

export interface UpdateTaskPayload {
    name?: string;
    description?: string | null;
    isChecked?: boolean;
    isArchived?: boolean;
}

export const tasksApi = {
    listForProject: (projectId: string) =>
        apiClient<Task[]>(`${ListProjects}/${projectId}${TasksBase}`),
    create: (payload: CreateTaskPayload) =>
        apiClient<Task>(CreateTask, {
            method: 'POST',
            body: payload,
        }),
    get: (taskId: string) =>
        apiClient<Task>(`${TasksBase}/${taskId}`),
    update: (taskId: string, payload: UpdateTaskPayload) =>
        apiClient<Task>(`${TasksBase}/${taskId}`, {
            method: 'PATCH',
            body: payload,
        }),
};