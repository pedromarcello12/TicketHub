import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // Em dev podemos usar o Gateway (porta 5000) como ponto único de entrada,
      // ou manter os serviços diretos para facilitar debug local sem Docker.
      // Mantenha GATEWAY=true nas variáveis de ambiente para usar o gateway.
      ...(process.env.GATEWAY
        ? {
            // ── Via Gateway (Docker Compose) ─────────────────────────────────
            '/api': {
              target: 'http://localhost:5000',
              changeOrigin: true,
            },
            '/hubs': {
              target: 'http://localhost:5000',
              changeOrigin: true,
              ws: true,
            },
          }
        : {
            // ── Serviços diretos (dev sem Docker) ────────────────────────────
            '/api/auth': {
              target: 'http://localhost:5004',
              changeOrigin: true,
            },
            '/api/eventos': {
              target: 'http://localhost:5275',
              changeOrigin: true,
            },
            '/api/ingressos': {
              target: 'http://localhost:5066',
              changeOrigin: true,
            },
            '/api/pagamentos': {
              target: 'http://localhost:5123',
              changeOrigin: true,
            },
            '/hubs': {
              target: 'http://localhost:5123',
              changeOrigin: true,
              ws: true, // habilita proxy de WebSocket para SignalR
            },
          }),
    },
  },
})
