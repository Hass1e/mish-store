import React, { useState, useEffect } from 'react';
import './AuthModal.css';
import { apiFetch } from '../../utils/api';
import { useAuth } from '../../context/AuthContext'; // ← ИМПОРТИРУЕМ ХУК!

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const AuthModal: React.FC<AuthModalProps> = ({ isOpen, onClose }) => {
  const [isLoginMode, setIsLoginMode] = useState<boolean>(true);
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isClosing, setIsClosing] = useState<boolean>(false);
  
  // Получаем login из контекста авторизации
  const { login } = useAuth(); 

  useEffect(() => {
    if (isOpen) {
      setIsClosing(false);
    }
  }, [isOpen]);

  if (!isOpen && !isClosing) return null;

  const handleCloseWithAnimation = () => {
    setIsClosing(true);
    setTimeout(() => {
      onClose();
      setIsClosing(false);
    }, 400);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsLoading(true);

    const requestData = { username, password };
    const endpoint = isLoginMode ? '/api/auth/login' : '/api/auth/register';

    try {
      const response = await apiFetch(endpoint, {
        method: 'POST',
        body: JSON.stringify(requestData),
      });

      if (response.ok) {
        const data = await response.json();
        
        // Вызываем login ИЗ КОНТЕКСТА (через useAuth)
        login({ 
          userId: data.userId, 
          username: data.username, 
          role: data.role,
          createdAt: data.createdAt

        });

        setUsername('');
        setPassword('');
        handleCloseWithAnimation(); 
      } else {
        if (response.status === 429) return;

        const errorText = await response.text();
        alert(`Ошибка: ${errorText || response.statusText}`);
      }
    } catch (error) {
      console.error('Ошибка сети:', error);
      alert('Не удалось связаться с сервером.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if ((e.target as HTMLElement).classList.contains('auth-overlay')) {
      handleCloseWithAnimation();
    }
  };

  return (
    <div className={`auth-overlay ${isClosing ? 'closing' : ''}`} onClick={handleOverlayClick}>
      <div className="auth-modal">
        <button type="button" className="auth-close-btn" onClick={handleCloseWithAnimation}>&times;</button>
        
        <h2 className="auth-title">{isLoginMode ? 'Вход' : 'Регистрация'}</h2>
        
        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="auth-input-group">
            <input 
              type="text" 
              required 
              placeholder=" " 
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
            <label>Имя пользователя</label>
          </div>
          
          <div className="auth-input-group">
            <input 
              type="password" 
              required 
              placeholder=" " 
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <label>Пароль</label>
          </div>
          
          <button type="submit" className="auth-submit-btn" disabled={isLoading}>
            {isLoading ? 'Загрузка...' : isLoginMode ? 'Войти' : 'Создать аккаунт'}
          </button>
        </form>
        
        <div className="auth-switch">
          <span>{isLoginMode ? 'Нет аккаунта?' : 'Уже есть аккаунт?'}</span> 
          <button 
            type="button" 
            className="auth-link-btn" 
            onClick={() => setIsLoginMode(!isLoginMode)}
          >
            {isLoginMode ? 'Зарегистрироваться' : 'Войти'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default AuthModal;