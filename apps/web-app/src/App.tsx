import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router-dom";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1, // Évite de boucler sur des erreurs serveur
      refetchOnWindowFocus: false, // Plus calme pour une app desktop
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {/* Tes routes viendront ici */}
        <h1>TaskForce Initialisée</h1>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
