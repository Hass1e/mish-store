// src/hooks/usePermissions.ts
import { useMemo } from 'react';
import { jwtDecode } from 'jwt-decode'; // npm install jwt-decode
import { useAuth } from '../context/AuthContext';

interface JwtPayload {
  permissions?: string;
  role?: string;
}

export function usePermissions() {
  const { user } = useAuth();
  const token = document.cookie
    .split('; ')
    .find(row => row.startsWith('accessToken='))
    ?.split('=')[1];

  const permissions = useMemo(() => {
    if (!token) return [];
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return decoded.permissions ? decoded.permissions.split(',') : [];
    } catch {
      return [];
    }
  }, [token]);

  const hasPermission = (permission: string) => permissions.includes(permission);
  const hasAnyPermission = (perms: string[]) => perms.some(p => permissions.includes(p));

  const isAdmin = user?.role?.toLowerCase() === 'admin';

  return {
    permissions,
    hasPermission: (permission: string) => isAdmin || hasPermission(permission),
    hasAnyPermission: (perms: string[]) => isAdmin || hasAnyPermission(perms)
  };
}