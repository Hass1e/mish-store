// src/context/AuthContext.tsx
import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { apiFetch } from '../utils/api';

interface User {
  userId: number;
  username: string;
  role: string;
  createdAt?: string; 
}

interface AuthContextType {
  user: User | null;
  login: (user: User) => void;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  
useEffect(() => {
  const checkSession = async () => {
    try {
      const res = await apiFetch('/api/auth/me'); 
      
      // ✅ Явно проверяем статус 429
      if (res.status === 429) {
        console.warn('⏳ Слишком много запросов. Проверка сессии пропущена.');
        // Не ставим setUser(null), чтобы не разлогинивать пользователя мгновенно
        // Или можно показать тост-уведомление
        return; 
      }

      if (res.ok) {
        const data = await res.json();
        setUser(data);
      } else {
        // Для 401/403/500 логика остается прежней
        setUser(null);
      }
    } catch (error) {
      // Сюда попадают только реальные сетевые ошибки (офлайн, DNS и т.д.)
      console.error('Ошибка сети при проверке сессии:', error);
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  };

  checkSession();
}, []);

  const login = (newUser: User) => {
    setUser(newUser);
  };

  const logout = async () => {
    const response = await apiFetch('/api/auth/logout', { method: 'POST' });
    if (!response.ok) {
      throw new Error(`Не удалось завершить сессию (${response.status})`);
    }

    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      login, 
      logout, 
      isAuthenticated: !!user,
      isLoading 
    }}>
      {!isLoading && children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}