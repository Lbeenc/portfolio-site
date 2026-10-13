import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  server: {
    // During `npm run dev`, forward /api calls to the ASP.NET server
    // (port comes from server/Portfolio.Api/Properties/launchSettings.json).
    proxy: {
      '/api': 'http://localhost:5115',
    },
  },

  build: {
    // `npm run build` writes the finished site straight into the ASP.NET app's
    // wwwroot folder, so the C# server can host the API and the site together.
    outDir: '../server/Portfolio.Api/wwwroot',
    emptyOutDir: true,
  },
})
