import { createFileRoute } from '@tanstack/react-router';
import { AcceptInvitation } from '@/pages/AcceptInvitation/AcceptInvitation';

export const Route = createFileRoute('/_protected/invite/$token')({
    component: AcceptInvitation,
});