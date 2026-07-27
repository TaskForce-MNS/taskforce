import { apiClient } from '@/api/client';
import { CreateProject, ListProjects, PutProject, PatchProject, GetProject } from '@/api/config';

export interface Project {
    id: string;
    name: string;
    description: string | null;
    colorHex: string | null;
    imageUrl: string | null;
    createdById: string;
    createdAt: string;
    updatedAt: string;
}

export interface CreateProjectPayload {
    name: string;
    description?: string;
    colorHex?: string;
    imageUrl?: string;
}

export interface PutProjectPayload {
    name: string;
    description: string | null;
    colorHex: string | null;
    imageUrl: string | null;
}


export interface PatchProjectPayload {
    name?: string;
    description?: string;
    colorHex?: string;
    imageUrl?: string;
}

export const projectsApi = {
    list: () =>
        apiClient<Project[]>(ListProjects),

    getById: (id: string) =>
        apiClient<Project>(`${GetProject}/${id}`),

    create: (payload: CreateProjectPayload) =>
        apiClient<Project>(CreateProject, {
            method: 'POST',
            body: payload,
        }),

    put: (id: string, payload: PutProjectPayload) =>
        apiClient<Project>(`${PutProject}/${id}`, {
            method: 'PUT',
            body: payload,
        }),

    patch: (id: string, payload: PatchProjectPayload) =>
        apiClient<Project>(`${PatchProject}/${id}`, {
            method: 'PATCH',
            body: payload,
        }),
};