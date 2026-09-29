import { apiClient } from '@/api/client';
import { ListProjects, TasksBase, CreateTask } from '@/api/config';

export const TaskDifficulty = {
    None: 0,
    Simple: 1,
    Medium: 2,
    Complex: 3
} as const;

export type TaskDifficulty = (typeof TaskDifficulty)[keyof typeof TaskDifficulty];

export interface Task {
    id: string;
    name: string;
    description: string | null;
    isChecked: boolean;
    isArchived: boolean;
    createdAt: string;
    projectId: string;
    dueDate?: string | null;
    assigneeId?: string | null;
    assigneeName?: string | null;
    difficulty: TaskDifficulty;
    storyPoints: number;
    targetWeek: string | null;
    requiredDomains: string[];
}

export interface CreateTaskPayload {
    name: string;
    description?: string | null;
    projectId: string;
    difficulty?: TaskDifficulty;
    requiredDomains?: string[];
    dueDate?: string | null;
}

export interface UpdateTaskPayload {
    name?: string;
    description?: string | null;
    isChecked?: boolean;
    isArchived?: boolean;
    assigneeId?: string | null;
    dueDate?: string | null;
    difficulty?: TaskDifficulty;
    requiredDomains?: string[];
    unassignTask?: boolean;
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