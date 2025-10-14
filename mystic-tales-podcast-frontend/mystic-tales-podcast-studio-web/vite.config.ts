import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import dotenv from 'dotenv'
import path from 'path'

dotenv.config()
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  css: {
    preprocessorOptions: {
      scss: {
        additionalData: `
          @use "src/styles/variables/app-variables" as app_vars;
          @use "src/styles/variables/FunctionArea/function-side-bar-variables" as functionSideBar_vars;
          @use "src/styles/variables/MainArea/chat-box-variables" as chatBox_vars;
          @use "src/styles/variables/MainArea/chat-box-info-variables" as chatBoxInfo_vars;
          @use "src/styles/variables/MainArea/room-browser-variables" as roomBrowser_vars;
        `,
      },
    },
  },
  resolve: {
    alias: {
      src: path.resolve(__dirname, 'src'),
    },
  },
  server: {
      port: process.env.VITE_PORT ? Number(process.env.VITE_PORT) : 3000,
      proxy: {
        // https://vitejs.dev/config/server-options.html
      },
    },
})
