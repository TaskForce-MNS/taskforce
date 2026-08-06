import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { tanstackRouter } from '@tanstack/router-plugin/vite';
import path from 'node:path'
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [tanstackRouter({ routesDirectory: './src/router/routes' }), react(), tailwindcss()],
  clearScreen: false,
  base: "./",
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
    strictPort: true,
    allowedHosts: [
      'app.taskforce.local',
      'app.staging.taskforce.stagiairesmns.fr',
    ]
  },
  build: {
    target: process.env.TAURI_PLATFORM == "windows" ? "chrome105" : "es2020",
    minify: !process.env.TAURI_DEBUG ? "esbuild" : false,
    sourcemap: !!process.env.TAURI_DEBUG,
  },
});
