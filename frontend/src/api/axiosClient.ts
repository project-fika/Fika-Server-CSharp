import axios from 'axios';
import { authEvents } from './authEvents';

export const api = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request Interceptor: Attach Token
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('access_token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Response Interceptor: Catch 401s Globally
api.interceptors.response.use(
    (response) => response,
    (error) => {
        // If receiving 401 from any protected API endpoint
        if (error.response?.status === 401) {
            // Do not emit logout if the failure came from trying to log in!
            const isLoginRequest = error.config?.url?.endsWith('/auth/login');
            if (!isLoginRequest) {
                authEvents.emitUnauthenticated();
            }
        }
        return Promise.reject(error);
    },
);
