import { QueryClient, QueryCache } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/useAuthStore';

export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (error instanceof Error && error.message.includes('401')) {
        useAuthStore.getState().logout();
      }
    },
  }),
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 1000 * 60 * 5, // 5 min
    },
  },
});