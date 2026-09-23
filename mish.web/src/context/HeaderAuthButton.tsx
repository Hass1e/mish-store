import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function HeaderAuthButton() {
  const { isAuthenticated, user, logout } = useAuth();

  if (isAuthenticated && user) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
        <span>Привет, {user.username}!</span>
        <Link to="/profile" className="btn btn-primary">Профиль</Link>
        <button onClick={logout} className="btn btn-outline">Выйти</button>
      </div>
    );
  }

  return (
    <Link to="/login" className="btn btn-primary">Войти</Link>
  );
}