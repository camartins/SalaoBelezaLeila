import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
  },
  server: {
    port: 5173,
    proxy: {
      '^/(Usuario|Autenticacao|Agendamento|Servico|Dashboard)': {
        target: 'http://localhost:5222',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
