import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5073', // <-- ТВОЙ ПОРТ API
        changeOrigin: true,
        secure: false,
      },
      '/swagger': {
        target: 'http://localhost:5073',
        changeOrigin: true,
        secure: false,
      },
      '/uploads': {
        target: 'http://localhost:5073',
        changeOrigin: true,
        secure: false,
      },
    }
  }
})