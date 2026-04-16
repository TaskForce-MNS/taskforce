import { useState, useEffect, type SubmitEvent } from 'react';
import { useAuthStore } from '@/stores/useAuthStore';
import { useNavigate } from '@tanstack/react-router';

export const Login = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [registerStep, setRegisterStep] = useState<1 | 2>(1);
  const [formData, setFormData] = useState({ firstname: '', lastname: '', title: '', experience: '' });
  const { loginWithPasskey, startRegistrationStep1,
    finalizeRegistrationStep2, isLoading, error, isAuthenticated } = useAuthStore();

  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated) {
      navigate({ to: '/dashboard' });
    }
  }, [isAuthenticated, navigate]);

  const handlePrimaryAuth = async () => {
    if (isLogin) {
      await loginWithPasskey();
    } else {
      const success = await startRegistrationStep1();
      if (success) {
        setRegisterStep(2);
      }
    }
  }
  
  const handleFinalSubmit = async (e: SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    await finalizeRegistrationStep2(formData);
  }

  return (
    < div className="relative flex min-h-screen w-full items-center justify-center overflow-hidden p-m bg-gradient-to-br from-primary-dark to-secondary-dark" >


      <div className="absolute inset-0 z-0 flex items-center justify-center px-[10px]">
 
        <svg
          xmlns="http://www.w3.org/2000/svg"
          className="h-auto w-full max-w-[600px] text-white-accent-light"
          viewBox="0 0 202 203"
          fill="none"
        >
          <path
            fillRule="evenodd"
            clipRule="evenodd"
            d="M55.71 59.0706C44.36 44.3506 28.72 34.9004 7.20996 34.9004C20.16 52.0704 37.12 58.8706 55.71 59.0706Z"
            fill="currentColor"
          />
          <path
            fillRule="evenodd"
            clipRule="evenodd"
            d="M145.7 59.0706C157.05 44.3506 172.69 34.9004 194.2 34.9004C181.25 52.0704 164.29 58.8706 145.7 59.0706Z"
            fill="currentColor"
          />
          <path
            fillRule="evenodd"
            clipRule="evenodd"
            d="M100.71 37.5C105.23 30.83 129.51 0.959963 201.35 0.959963V20.3401C124.8 20.3401 112.02 136.4 110.03 202.24H91.3799C89.3899 136.4 76.6096 20.3401 0.0595703 20.3401V0.959963C71.8996 0.949963 96.19 30.83 100.71 37.5Z"
            fill="currentColor"
          />
        </svg>
      </div>
      <div className="relative z-10 flex w-full max-w-md flex-col items-center rounded-medium border border-white-accent-dark/20 bg-black-accent-dark/60 p-2xl shadow-xl backdrop-blur-xl">

        <div className="mb-l flex h-16 w-16 items-center justify-center rounded-medium bg-gradient-to-br from-primary-default to-secondary-default shadow-lg">
          {registerStep === 2 ? (
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
          {isLogin ? 'Bon retour' : registerStep === 1 ? 'S\'inscrire à TaskForce' : 'Création du profil'}
        </h1>
        <p className="mb-xl text-center font-text text-sm text-white-accent-light/60">
          {isLogin || registerStep === 1
            ? "Authentification via Passkey. Aucun mot de passe requis."
            : "Dites-nous en plus sur vous."}
        </p>

        {error && (
          <div role="alert" className="mb-l w-full rounded-medium border border-error/30 bg-error/10 p-m text-center font-text text-sm text-white-accent-light backdrop-blur-sm">
            {error}
          </div>
        )}
        {(!isLogin && registerStep === 2) ? (
          <form onSubmit={handleFinalSubmit} className="w-full flex flex-col gap-m animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div className="flex flex-col gap-s">
              <label className="font-text text-sm text-white-accent-dark">Prénom</label>
              <input
                type="text"
                required
                placeholder=""
                value={formData.firstname}
                onChange={(e) => setFormData({ ...formData, firstname: e.target.value })}
                className="w-full rounded-medium border border-white-accent-dark/30 bg-black-accent-default/50 p-m font-text text-white-accent-light outline-none focus:border-primary-default focus:ring-1 focus:ring-primary-default"
              />
            </div>
            <div className="flex flex-col gap-s">
              <label className="font-text text-sm text-white-accent-dark">Nom de famille</label>
              <input
                type="text"
                required
                placeholder=""
                value={formData.lastname}
                onChange={(e) => setFormData({ ...formData, lastname: e.target.value })}
                className="w-full rounded-medium border border-white-accent-dark/30 bg-black-accent-default/50 p-m font-text text-white-accent-light outline-none focus:border-primary-default focus:ring-1 focus:ring-primary-default"
              />
            </div>
            <div className="flex flex-col gap-s">
              <label className="font-text text-sm text-white-accent-dark">Titre de votre poste</label>
              <input
                type="text"
                required
                placeholder="ex: Développeur Fullstack"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                className="w-full rounded-medium border border-white-accent-dark/30 bg-black-accent-default/50 p-m font-text text-white-accent-light outline-none focus:border-primary-default focus:ring-1 focus:ring-primary-default"
              />
            </div>
            <div className="flex flex-col gap-s mb-s">
              <label className="font-text text-sm text-white-accent-dark">Niveau d'expérience</label>
              <select
                required
                value={formData.experience}
                onChange={(e) => setFormData({ ...formData, experience: e.target.value })}
                className="w-full rounded-medium border border-white-accent-dark/30 bg-black-accent-default/50 p-m font-text text-white-accent-light outline-none focus:border-primary-default focus:ring-1 focus:ring-primary-default"
              >
                <option value="" disabled>Combien d'année d'expérience avez-vous ?</option>
                <option value="1">1</option>
                <option value="2">2</option>
                <option value="3">3</option>
                <option value="4">4</option>
              </select>
            </div>
            <button
              type="submit"
              disabled={isLoading}
              className="group relative flex w-full items-center justify-center gap-s rounded-medium bg-primary-default py-3 font-text font-bold text-white-accent-light transition-all hover:bg-primary-dark disabled:opacity-50"
            >
              {isLoading ? 'Finalisation...' : 'Terminer mon inscription'}
            </button>
          </form>
        ) : (
          <button
            onClick={handlePrimaryAuth}
            disabled={isLoading}
            aria-busy={isLoading}
            className="group relative flex w-full items-center justify-center gap-s rounded-medium bg-white-accent-light py-3 font-text font-bold text-black-accent-default shadow-[0_0_20px_rgba(255,255,255,0.15)] transition-all duration-300 hover:scale-[1.01] hover:bg-white-accent-default disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:scale-100"
          >
            {isLoading ? (
              <div aria-hidden="true" className="h-5 w-5 animate-spin rounded-button border-2 border-black-accent-default border-t-transparent"></div>
            ) : (
              <>
                <svg aria-hidden="true" className="h-5 w-5 transition-transform group-hover:scale-105" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                </svg>
                {isLogin ? 'Se connecter avec Passkey' : 'Créer mon Passkey'}
              </>
            )}
          </button>
        )}
        {/* Login/Register */}
        {registerStep === 1 && (
          <div className="mt-xl w-full border-t border-white-accent-dark/50 pt-l text-center">
            <button
              onClick={() => setIsLogin(!isLogin)}
              disabled={isLoading}
              className="font-text text-sm font-medium text-white-accent-dark transition-colors hover:text-white-accent-light"
            >
              {isLogin
                ? "Nouveau sur TaskForce ? S'inscrire"
                : "Déjà un compte ? Se connecter"}
            </button>
          </div>
        )}
      </div>
    </div >
  );
};