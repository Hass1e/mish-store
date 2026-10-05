// mish.web/vite.config.prod.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  // Для монолита base всегда должен быть '/' или './'
  base: '/', 
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    // Отключаем sourcemaps для продакшена (опционально, но полезно)
    sourcemap: false 
  }
})