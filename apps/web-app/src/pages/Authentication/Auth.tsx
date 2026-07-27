import { useState, useEffect } from 'react';
import { Input } from '@/components/atoms/Input';
import { Button } from '@/components/atoms/Button';
import { useAuthStore } from '@/stores/useAuthStore';
import { useNavigate } from '@tanstack/react-router';
import { Alert } from '@/components/atoms/Alert';
import { AuthLayout } from '@/templates/AuthLayout';
import { useQueryClient } from '@tanstack/react-query';

export const Auth = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({ firstname: '', lastname: '', title: '', experience: '' });
  const { loginWithPasskey,
    registerWithPasskey, isLoading, error } = useAuthStore();

  const navigate = useNavigate();

  const queryClient = useQueryClient();

  const handleLogin = async () => {
    const success = await loginWithPasskey();
    if (success) {
      queryClient.clear();
      navigate({ to: '/dashboard' });
    }
  };

  const handleRegisterSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    await registerWithPasskey(formData);

    if (useAuthStore.getState().isAuthenticated) {
      navigate({ to: '/dashboard' });
    }
  };

  const PasskeyIcon = (
    <svg aria-hidden="true" className="h-5 w-5 transition-transform group-hover:scale-105" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
    </svg>
  );

  return (
    <AuthLayout>
      <div className="mb-l flex h-16 w-16 items-center justify-center rounded-medium bg-gradient-to-br from-primary-default to-secondary-default shadow-lg">
        {isLogin ? (
          <svg aria-hidden="true" className="h-8 w-8 text-white-accent-light" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
        ) : (
          <svg aria-hidden="true" className="h-8 w-8 text-white-accent-light" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
          </svg>
        )}
      </div>

      <h1 className="mb-s text-center font-title text-3xl font-bold text-white-accent-light">
        {isLogin ? 'Bon retour' : 'S\'inscrire à TaskForce'}
      </h1>
      <p className="mb-xl text-center font-text text-sm text-white-accent-light/60">
        {isLogin
          ? "Authentification via Passkey. Aucun mot de passe requis."
          : "Dites-nous en plus sur vous."}
      </p>

      {error && (
        <Alert variant="error" title="Authentication Failed" className="mb-4">
          {error}
        </Alert>
      )}
      {!isLogin ? (
        <form onSubmit={handleRegisterSubmit} className="w-full flex flex-col gap-m">
          <div className="flex flex-col sm:flex-row gap-m w-full">
            <Input
              label="Prénom"
              type="text"
              required
              value={formData.firstname}
              onChange={(e) => setFormData({ ...formData, firstname: e.target.value })}
              inputSize="md"
            />
            <Input
              label="Nom de famille"
              type="text"
              required
              value={formData.lastname}
              onChange={(e) => setFormData({ ...formData, lastname: e.target.value })}
              inputSize="md"
            />
          </div>

          <Input
            label="Titre de votre poste"
            type="text"
            required
            placeholder="ex: Développeur Fullstack"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            inputSize="md"
          />

          {/* Select (Tu pourras plus tard créer un composant <Select /> personnalisé sur le même modèle que <Input />) */}
          <div className="flex flex-col gap-1.5 mb-s w-full">
            <label className="text-sm font-medium text-white-accent-default select-none">
              Niveau d'expérience
            </label>
            <select
              required
              value={formData.experience}
              onChange={(e) => setFormData({ ...formData, experience: e.target.value })}
              className="h-12 w-full rounded-large border border-slate-700 bg-transparent px-3 text-base text-white-accent-light transition-all duration-200 ease-in-out hover:border-primary-dark focus:border-primary-default focus:outline-none focus:ring-2 focus:ring-primary-default/20 disabled:cursor-not-allowed disabled:bg-black-accent-light disabled:opacity-50"
            >
              <option value="1" className="text-black">1 an</option>
              <option value="2" className="text-black">2 ans</option>
              <option value="3" className="text-black">3 ans</option>
              <option value="4" className="text-black">4 ans et +</option>
            </select>
          </div>

          <Button
            type="submit"
            variant="passkey"
            size="md"
            fullWidth
            isLoading={isLoading}
            leftIcon={PasskeyIcon}
            className="mt-2"
          >
            Terminer mon inscription
          </Button>

        </form>
      ) : (
        <Button
          variant="passkey"
          size="md"
          fullWidth
          onClick={handleLogin}
          isLoading={isLoading}
          leftIcon={PasskeyIcon}
        >
          {isLogin ? 'Se connecter avec Passkey' : 'Créer mon Passkey'}
        </Button>
      )}
      {/* Login/Register */}
      <div className="mt-xl w-full border-t border-white-accent-dark/50 pt-l text-center">
        <Button
          variant="link"
          onClick={() => {
            setIsLogin(!isLogin);
          }}
          disabled={isLoading}
          className="text-white-accent-dark hover:text-white-accent-light text-sm font-medium transition-colors no-underline"
        >
          {isLogin
            ? "Nouveau sur TaskForce ? S'inscrire"
            : "Déjà un compte ? Se connecter"}
        </Button>
      </div>
    </AuthLayout >
  );
};