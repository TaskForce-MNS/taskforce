const protocol = import.meta.env.VITE_API_PROTOCOL
const host = import.meta.env.VITE_API_HOST
const path = import.meta.env.VITE_API_PATH
const suffix = import.meta.env.VITE_API_SUFFIX
const version = import.meta.env.VITE_API_VERSION
export const API_BASE_URL = `${protocol}://${host}${path}${version}${suffix}`;

// AUTHENTICATION
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
// REFRESH TOKEN
export const refreshToken = import.meta.env.VITE_REFRESH_TOKEN_PATH

// PROJECT
export const CreateProject = import.meta.env.VITE_CREATE_PROJECT
export const ListProjects = import.meta.env.VITE_LIST_PROJECTS
export const GetProject = import.meta.env.VITE_GET_PROJECT
export const PutProject = import.meta.env.VITE_PUT_PROJECT
export const PatchProject = import.meta.env.VITE_PATCH_PROJECT