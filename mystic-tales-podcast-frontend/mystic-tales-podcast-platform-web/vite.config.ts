import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import tailwindcss from "@tailwindcss/vite";

// https://vite.dev/config/
export default defineConfig({
  base: "./", // 👈 cực kỳ quan trọng
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    proxy: {
      "/api": {
        target: "https://fast-scorpion-strictly.ngrok-free.app",
        changeOrigin: true,
        secure: false,
      },
    },
    host: true,
    allowedHosts: true,
  },
  optimizeDeps: {
    include: ["quill"],
  },
  build: {
    commonjsOptions: {
      include: [/quill/, /node_modules/],
      transformMixedEsModules: true,
    },
  },
});
