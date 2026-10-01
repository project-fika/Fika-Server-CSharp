import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
    allowedRoles?: string[];
}

export function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
    const { isAuthenticated, hasRole } = useAuth();
    const location = useLocation();

    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (allowedRoles && allowedRoles.length > 0 && !hasRole(...allowedRoles)) {
        return <Navigate to="/dashboard" replace />;
    }

    return <Outlet />;
}
