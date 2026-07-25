import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api/auth': {
        target: 'http://localhost:5004',
        changeOrigin: true,
      },
      '/api/eventos': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      '/api/ingressos': {
        target: 'http://localhost:5002',
        changeOrigin: true,
      },
      '/api/pagamentos': {
        target: 'http://localhost:5003',
        changeOrigin: true,
      },
    },
  },
})
