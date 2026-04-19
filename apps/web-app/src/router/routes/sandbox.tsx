import { createFileRoute } from '@tanstack/react-router';
import { Sandbox } from '@/pages/sandbox/Sandbox';

export const Route = createFileRoute('/sandbox')({
    component: Sandbox,
});