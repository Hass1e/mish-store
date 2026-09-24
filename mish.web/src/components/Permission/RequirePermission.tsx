// src/components/RequirePermission.tsx
import type { ReactNode } from 'react';
import { usePermissions } from '../../hooks/usePermissions'; // Твой хук из прошлого шага
import { Navigate } from 'react-router-dom';

interface RequirePermissionProps {
  permission: string;
  children: ReactNode;
  fallbackPath?: string;
}

export function RequirePermission({ 
  permission, 
  children, 
  fallbackPath = '/catalog' 
}: RequirePermissionProps) {
  const { hasPermission } = usePermissions();

  if (!hasPermission(permission)) {
    // Если прав нет — редиректим на каталог (или страницу 403)
    return <Navigate to={fallbackPath} replace />;
  }

  // Если права есть — рендерим защищенный контент
  return <>{children}</>;
}