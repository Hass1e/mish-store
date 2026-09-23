
import './MainInfo.css';
import { useState, useEffect } from 'react';
import { apiFetch } from '../../../../../utils/api';

interface UserProfile {
  username: string;
  role: string;
  createdAt: string;
}

export function MainInfo() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const res = await apiFetch('/api/users/profile');
        if (res.ok) {
          const data = await res.json();
          setProfile(data);
        }
      } catch (err) {
        console.error('Ошибка загрузки профиля:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, []);

  if (loading) return <div>Загрузка...</div>;
  if (!profile) return <div>Не удалось загрузить профиль</div>;

  const registrationDate = new Date(profile.createdAt).toLocaleDateString('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  return (
    <div className="main-info">
      <h2 className="section-title">Личные данные</h2>
      <div className="info-grid">
        <div className="info-item">
          <span className="info-label">Имя пользователя</span>
          <span className="info-value">{profile.username}</span>
        </div>
        <div className="info-item">
          <span className="info-label">Роль</span>
          <span className="info-value">
            {profile.role === 'Admin' ? 'Администратор' : 'Пользователь'}
          </span>
        </div>
        <div className="info-item">
          <span className="info-label">Дата регистрации</span>
          <span className="info-value">{registrationDate}</span>
        </div>
      </div>
    </div>
  );
}