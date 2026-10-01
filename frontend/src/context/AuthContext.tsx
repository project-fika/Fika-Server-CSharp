import { jwtDecode } from 'jwt-decode';
import type { ReactNode } from 'react';
import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { authEvents } from '../api/authEvents';
import type { AuthContextType, JwtPayload, User } from '../types/auth';

const AuthContext = createContext<AuthContextType | null>(null);

function parseUserFromToken(token: string | null): User | null {
    if (!token) return null;
    try {
        const decoded = jwtDecode<JwtPayload>(token);

        const rawRoles = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || [];
        const roles = Array.isArray(rawRoles) ? rawRoles : [rawRoles];

        const username = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User';

        return {
            id: decoded.sub || '',
            username,
            roles,
        };
    } catch {
        return null;
    }
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(() => localStorage.getItem('access_token'));
    const [user, setUser] = useState<User | null>(() => parseUserFromToken(token));

    const login = useCallback((newToken: string, _username: string) => {
        localStorage.setItem('access_token', newToken);
        setToken(newToken);
        setUser(parseUserFromToken(newToken));
    }, []);

    const logout = useCallback(() => {
        localStorage.removeItem('access_token');
        setToken(null);
        setUser(null);
    }, []);

    const hasRole = useCallback(
        (...roles: string[]): boolean => {
            if (!user) return false;
            return roles.some((role) => user.roles.includes(role));
        },
        [user],
    );

    useEffect(() => {
        const unsubscribe = authEvents.onUnauthenticated(() => {
            logout();
        });
        return () => unsubscribe();
    }, [logout]);

    return <AuthContext.Provider value={{ token, user, isAuthenticated: !!token, login, logout, hasRole }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth must be used within an AuthProvider');
    return context;
}
