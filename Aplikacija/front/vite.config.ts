import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    tailwindcss(),
  ],

  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },

  server: {
    host: true,
    strictPort: false,
    allowedHosts: ['.ngrok-free.app'],
    proxy: {
      '/api': {
        target: 'http://localhost:5266',
        changeOrigin: true
      },
      '/hubs': {
        target: 'http://localhost:5039',
        changeOrigin: true,
        ws: true
      }
    }
  },

  css: {
    preprocessorOptions: {
      scss: {
        silenceDeprecations: ['import', 'global-builtin', 'color-functions', 'if-function'],
      },
    },
  },
})
