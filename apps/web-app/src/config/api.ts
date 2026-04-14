const protocol = import.meta.env.VITE_API_PROTOCOL
const host     = import.meta.env.VITE_API_HOST    
const path     = import.meta.env.VITE_API_PATH    
const suffix   = import.meta.env.VITE_API_SUFFIX  
const version  = import.meta.env.VITE_API_VERSION 
export const API_BASE_URL = `${protocol}://${host}${path}${suffix}${version}`;

// LOGIN
export const authn = import.meta.env.VITE_AUTH_PATH
export const login = import.meta.env.VITE_LOGIN_PATH
export const loginOptions = import.meta.env.VITE_LOGIN_OPTION

// AUTHN ME
export const authMe = import.meta.env.VITE_AUTH_ME_PATH

// LOGOUT
export const logout =import.meta.env.VITE_LOGOUT_PATH
