import { useState } from 'react';
import { apiFetch } from '../../../../../utils/api';
import './Security.css';

export function Security() {
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage(null);

    try {
      const res = await apiFetch('/api/users/change-password', {
        method: 'POST',
        body: JSON.stringify({ oldPassword, newPassword }),
      });

      if (res.ok) {
        setMessage({ type: 'success', text: 'Пароль успешно изменён!' });
        setOldPassword('');
        setNewPassword('');
      } else {
        const errorText = await res.text();
        setMessage({ type: 'error', text: errorText || 'Ошибка при смене пароля' });
      }
    } catch {
      setMessage({ type: 'error', text: 'Ошибка сети' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!window.confirm('Вы уверены? Это действие нельзя отменить!')) return;

    try {
      const res = await apiFetch('/api/users/account', { method: 'DELETE' });
      if (res.ok) {
        window.location.href = '/';
      } else {
        const error = await res.json().catch(() => null);
        alert(error?.message || `Не удалось удалить аккаунт (${res.status})`);
      }
    } catch (error) {
      alert(error instanceof Error ? error.message : 'Ошибка при удалении аккаунта');
    }
  };

  return (
    <div className="security">
      <h2 className="section-title">Безопасность</h2>

      {/* Смена пароля */}
      <form className="password-form" onSubmit={handlePasswordChange}>
        <h3 className="subsection-title">Сменить пароль</h3>
        
        <div className="form-group">
          <label>Старый пароль</label>
          <input
            type="password"
            value={oldPassword}
            onChange={(e) => setOldPassword(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label>Новый пароль</label>
          <input
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
            minLength={6}
          />
        </div>

        <button type="submit" className="btn-primary" disabled={isLoading}>
          {isLoading ? 'Сохранение...' : 'Сменить пароль'}
        </button>
      </form>

      {message && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}

      {/* Удаление аккаунта */}
      <div className="danger-zone">
        <h3 className="subsection-title danger">Удалить аккаунт</h3>
        <p className="danger-text">
          Это действие удалит все ваши данные без возможности восстановления.
        </p>
        <button className="btn-danger" onClick={handleDeleteAccount}>
          Удалить аккаунт
        </button>
      </div>
    </div>
  );
}