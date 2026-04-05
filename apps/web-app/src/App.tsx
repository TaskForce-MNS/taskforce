// import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
// import { BrowserRouter } from "react-router-dom";
// import dayjs from 'dayjs';

// const queryClient = new QueryClient({
//   defaultOptions: {
//     queries: {
//       retry: 1, // Évite de boucler sur des erreurs serveur
//       refetchOnWindowFocus: false, // Plus calme pour une app desktop
//     },
//   },
// });

// export default function App() {
//   return (
//     <QueryClientProvider client={queryClient}>
//       <BrowserRouter>
//         {/* Tes routes viendront ici */}
//         <h1>TaskForce Initialisée</h1>
//         <svg
//           xmlns="http://www.w3.org/2000/svg"
//           width="202"
//           height="203"
//           viewBox="0 0 202 203"
//           fill="none"
//         >
//           <path
//             fill-rule="evenodd"
//             clip-rule="evenodd"
//             d="M55.71 59.0706C44.36 44.3506 28.72 34.9004 7.20996 34.9004C20.16 52.0704 37.12 58.8706 55.71 59.0706Z"
//             fill="black"
//           />
//           <path
//             fill-rule="evenodd"
//             clip-rule="evenodd"
//             d="M145.7 59.0706C157.05 44.3506 172.69 34.9004 194.2 34.9004C181.25 52.0704 164.29 58.8706 145.7 59.0706Z"
//             fill="black"
//           />
//           <path
//             fill-rule="evenodd"
//             clip-rule="evenodd"
//             d="M100.71 37.5C105.23 30.83 129.51 0.959963 201.35 0.959963V20.3401C124.8 20.3401 112.02 136.4 110.03 202.24H91.3799C89.3899 136.4 76.6096 20.3401 0.0595703 20.3401V0.959963C71.8996 0.949963 96.19 30.83 100.71 37.5Z"
//             fill="black"
//           />
//         </svg>
//         <div style={{ padding: "20px" }}>
//           <p>Test d'un vrai paquet (Day.js) :</p>
//           <p>Nous sommes le : <strong>{dayjs().format('DD/MM/YYYY à HH:mm')}</strong></p> 
//         </div>
//       </BrowserRouter>
//     </QueryClientProvider>
//   );
// }
import { useState } from 'react';
import { startRegistration } from '@simplewebauthn/browser';

export default function App() {
  const [status, setStatus] = useState<string>("Prêt pour le test.");

  // Remplace par l'URL de ton API .NET (ex: https://localhost:7001 ou http://localhost:5000)
  const API_BASE_URL = "https://api.taskforce.local/api/v1/back/auth";

  const handleRegisterZeroKnowledge = async () => {
    try {
      setStatus("⏳ Étape 1 : Demande du défi au serveur...");
      
      // 1. On demande les options cryptographiques au backend
      const optionsResponse = await fetch(`${API_BASE_URL}/register/options`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
      });
      
      if (!optionsResponse.ok) throw new Error("Erreur lors de la récupération des options");
      const options = await optionsResponse.json();

      setStatus("👆 Étape 2 : En attente de ton empreinte / Windows Hello...");

      // 2. MAGIE FRONTEND : On réveille la biométrie de l'appareil !
      // startRegistration gère toute la conversion des ArrayBuffers automatiquement
      const attestationResponse = await startRegistration(options);

      setStatus("🔒 Étape 3 : Chiffrement du profil et envoi au serveur...");

      // 3. On simule les données Zéro-Connaissance que ton app générera plus tard
      const payload = {
        // En vrai, React chiffrera le nom/prénom ici. Là, on envoie une fausse donnée en Base64.
        EncryptedProfileBlob: btoa(JSON.stringify({ nom: "Test", prenom: "ZeroKnowledge" })), 
        Experience: "05", // Attention, ton validateur C# exige max 2 caractères !
        Title: "Développeur Fullstack",
        WebAuthnAttestationResponse: attestationResponse // La réponse signée par la puce de ton PC
      };

      // 4. On envoie tout au backend pour validation et sauvegarde
      const verificationResponse = await fetch(`${API_BASE_URL}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      const verificationResult = await verificationResponse.json();

      if (verificationResponse.ok) {
        setStatus(`✅ SUCCÈS ! Identité Zéro-Connaissance créée (ID: ${verificationResult.identityId})`);
      } else {
        setStatus(`❌ ERREUR BACKEND : ${JSON.stringify(verificationResult)}`);
      }

    } catch (error: any) {
      console.error(error);
      setStatus(`❌ ERREUR LOCALE : ${error.message}`);
    }
  };

  return (
    <div style={{ padding: '50px', fontFamily: 'sans-serif', textAlign: 'center' }}>
      <h1>Test Laboratoire : Passkeys Zéro-Connaissance 🛡️</h1>
      
      <button 
        onClick={handleRegisterZeroKnowledge}
        style={{ padding: '15px 30px', fontSize: '18px', cursor: 'pointer', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '8px' }}
      >
        Créer mon Identité (Passkey)
      </button>

      <div style={{ marginTop: '30px', padding: '20px', backgroundColor: '#755454', borderRadius: '8px' }}>
        <strong>Statut :</strong> <br/> {status}
      </div>
    </div>
  );
}
