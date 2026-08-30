export interface JwtPayload {
    sub?: string;
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
    role?: string | string[];
    exp?: number;
}

export interface User {
    id: string;
    username: string;
    roles: string[];
}

export interface AuthContextType {
    token: string | null;
    user: User | null;
    isAuthenticated: boolean;
    login: (token: string, username: string) => void;
    logout: () => void;
    hasRole: (...roles: string[]) => boolean;
}
