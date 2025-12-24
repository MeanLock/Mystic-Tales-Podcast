import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import dotenv from "dotenv";
import path from "path";
import tailwindcss from "@tailwindcss/vite";

dotenv.config();
// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  base: "/",
  css: {
    preprocessorOptions: {
      scss: {
        additionalData: `
          @use "./src/styles/variables/app-variables.scss" as app_vars;
          @use "./src/styles/variables/FunctionArea/function-side-bar-variables" as functionSideBar_vars;
          @use "./src/styles/variables/MainArea/chat-box-variables" as chatBox_vars;
          @use "./src/styles/variables/MainArea/chat-box-info-variables" as chatBoxInfo_vars;
          @use "./src/styles/variables/MainArea/room-browser-variables" as roomBrowser_vars;
        `,
      },
    },
  },
  resolve: {
    alias: {
      src: path.resolve(__dirname, "src"),
      "@": path.resolve(__dirname, "./src"),
    },
  },
  optimizeDeps: {
    include: ['quill']
  },
  build: {
    commonjsOptions: {
      include: [/quill/, /node_modules/],
      transformMixedEsModules: true
    }
  },
  server: {
    port: process.env.VITE_PORT ? Number(process.env.VITE_PORT) : 3000,
    allowedHosts: [
      'cd9fd37fb126.ngrok-free.app'
    ],
    proxy: {
      // https://vitejs.dev/config/server-options.html
    },
  },
});
