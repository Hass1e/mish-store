import { useState, useEffect } from 'react';
import { apiFetch } from './../../../utils/api';
import './ChangeRoles.css';

interface Role {
  id: number;
  name: string;
  description?: string;
}

export function ChangeRoles() {
  const [username, setUsername] = useState('');
  const [roleId, setRoleId] = useState<number>(1);
  const [roles, setRoles] = useState<Role[]>([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Загружаем роли при открытии страницы
  useEffect(() => {
    const loadRoles = async () => {
      try {
        const res = await apiFetch('/api/roles');
        if (res.ok) setRoles(await res.json());
      } catch (err) {
        setError('Не удалось загрузить список ролей');
      }
    };
    loadRoles();
  }, []);

  const handleChangeRole = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('');
    setError('');
    setIsLoading(true);

    try {
      const res = await apiFetch('/api/roles/change-role', {
        method: 'PUT',
        body: JSON.stringify({ username, roleId }),
      });

      if (res.ok) {
        const data = await res.json();
        setMessage(data.message);
        setUsername('');
      } else {
        const errData = await res.json();
        setError(errData.message || 'Ошибка при смене роли');
      }
    } catch (err) {
      setError('Ошибка сети');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="admin-tool">
      <h2 className="admin-tool__title">Управление ролями</h2>
      
      <form onSubmit={handleChangeRole} className="admin-tool__form">
        <input
          type="text"
          placeholder="Никнейм пользователя"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
          className="admin-tool__input"
        />

        <select
          value={roleId}
          onChange={(e) => setRoleId(Number(e.target.value))}
          className="admin-tool__select"
        >
          {roles.map((r) => (
            <option key={r.id} value={r.id}>
              {r.name} {r.description ? `(${r.description})` : ''}
            </option>
          ))}
        </select>

        <button 
          type="submit" 
          disabled={isLoading}
          className="admin-tool__btn"
        >
          {isLoading ? 'Сохранение...' : 'Назначить роль'}
        </button>
      </form>

      {message && <p className="admin-tool__message admin-tool__message--success">{message}</p>}
      {error && <p className="admin-tool__message admin-tool__message--error">{error}</p>}
    </div>
  );
}