import React from 'react';
import { useAuthStore } from './stores/useAuthStore';

export const LoginButton: React.FC = () => {
  const { loginWithPasskey, isLoading, error, token, logout } = useAuthStore();

  if (token) {
    return (
      <div className="flex flex-col items-center gap-4">
        <div className="text-green-500 font-bold">✅ Connecté avec succès !</div>
        <button 
          onClick={logout}
          className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
        >
          Se déconnecter
        </button>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-4">
      <button 
        onClick={loginWithPasskey} 
        disabled={isLoading}
        className="px-6 py-3 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-all flex items-center gap-2"
      >
        {isLoading ? "Vérification..." : "🔐 Se connecter (FaceID / TouchID)"}
      </button>
      
      {error && <div className="text-red-500 text-sm">{error}</div>}
    </div>
  );
};