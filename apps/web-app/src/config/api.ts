const protocol = import.meta.env.VITE_API_PROTOCOL
const host     = import.meta.env.VITE_API_HOST    
const path     = import.meta.env.VITE_API_PATH    
const suffix   = import.meta.env.VITE_API_SUFFIX  
const version  = import.meta.env.VITE_API_VERSION 
export const API_BASE_URL = `${protocol}://${host}${path}${suffix}${version}`;
