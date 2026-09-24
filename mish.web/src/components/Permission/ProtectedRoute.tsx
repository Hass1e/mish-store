// src/components/ProtectedRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { usePermissions } from '../../hooks/usePermissions';

interface ProtectedRouteProps {
  requiredPermission: string;
  redirectPath?: string;
}

export function ProtectedRoute({ requiredPermission, redirectPath = '/' }: ProtectedRouteProps) {
  const { hasPermission } = usePermissions();

  if (!hasPermission(requiredPermission)) {
    return <Navigate to={redirectPath} replace />;
  }

  return <Outlet />;
}