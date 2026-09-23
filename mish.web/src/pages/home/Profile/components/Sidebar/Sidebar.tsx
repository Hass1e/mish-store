import { NavLink } from 'react-router-dom';
import { useAuth } from '../../../../../context/AuthContext';
import './Sidebar.css';

export function Sidebar() {
  const { user } = useAuth();

  // Первая буква имени для аватара
  const initial = user?.username?.charAt(0).toUpperCase() || '?';
  const isAdmin = user?.role === 'Admin';

  const navItems = [
    { to: '/profile', label: 'Главная', end: true },
    { to: '/profile/security', label: 'Безопасность' },
    { to: '/profile/orders', label: 'Заказы' },
    { to: '/profile/support', label: 'Поддержка' },
  ];

  return (
    <aside className="sidebar">
      {/* Аватар */}
      <div className={`sidebar-avatar ${isAdmin ? 'admin' : 'user'}`}>
        {initial}
      </div>

      {/* Имя и роль */}
      <div className="sidebar-user-info">
        <h3 className="sidebar-username">{user?.username}</h3>
        <span className="sidebar-role">{isAdmin ? 'Администратор' : 'Пользователь'}</span>
      </div>

      {/* Навигация */}
      <nav className="sidebar-nav">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `sidebar-link ${isActive ? 'active' : ''}`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}