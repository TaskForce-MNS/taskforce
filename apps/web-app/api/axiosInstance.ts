import axios from "axios";

// Instance axios avec gestion centralisée des erreurs
const axiosInstance = axios.create({
  baseURL: "/api",
  withCredentials: true,
});

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    // Affiche un message utilisateur ou loggue l’erreur
    // alert('Erreur API : ' + (error.response?.data?.message || error.message));
    // Tu peux aussi router vers une page d’erreur globale
    return Promise.reject(error);
  },
);

export default axiosInstance;
