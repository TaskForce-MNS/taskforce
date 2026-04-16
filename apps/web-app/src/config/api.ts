const protocol = import.meta.env.VITE_API_PROTOCOL
const host = import.meta.env.VITE_API_HOST
const path = import.meta.env.VITE_API_PATH
const suffix = import.meta.env.VITE_API_SUFFIX
const version = import.meta.env.VITE_API_VERSION
export const API_BASE_URL = `${protocol}://${host}${path}${version}${suffix}`;

export const auth = import.meta.env.VITE_AUTH_PATH
// LOGIN
export const login = import.meta.env.VITE_LOGIN_PATH
export const authOptions = import.meta.env.VITE_AUTH_OPTIONS

// AUTHN ME
export const authMe = import.meta.env.VITE_AUTH_ME_PATH

// LOGOUT
export const logout = import.meta.env.VITE_LOGOUT_PATH

// REGISTER
export const register = import.meta.env.VITE_REGISTER_PATH
