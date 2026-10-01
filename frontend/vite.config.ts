import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
    plugins: [react()],
    server: {
        port: 5173,
        proxy: {
            '/api': {
                target: 'http://localhost:64579',
                changeOrigin: true,
                secure: false, // allows self-signed SSL certs during local debug
            },
        },
    },
});
