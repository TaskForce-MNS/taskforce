// @ts-check
import { defineConfig } from 'astro/config';

// https://astro.build/config
export default defineConfig({
  server: {
    host: '0.0.0.O',
    port: 4321
  },
  vite: {
    server: {
      allowedHosts: ['taskforce.local']
    }
  }
});