import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'node:path'
import dotenv from 'dotenv'
import tailwindcss from "@tailwindcss/vite";

dotenv.config()

export default defineConfig(() => {
  return {
    base: './',
    build: {
      outDir: 'build',
    },
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        src: path.resolve(__dirname, "src"),
        '@': path.resolve(__dirname, "./src"),
      },
      extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.scss'],
    },
    server: {
      port: process.env.VITE_PORT ? Number(process.env.VITE_PORT) : 3000,
      proxy: {
        // https://vitejs.dev/config/server-options.html
      },
    },
  }
})