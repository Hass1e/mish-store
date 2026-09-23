
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from '../../../context/AuthContext';
import { Sidebar } from './components/Sidebar/Sidebar';
import { MainInfo } from './components/MainInfo/MainInfo';
import { Security } from './components/Security/Security';
import { Orders } from './components/Orders/Order'; 
import { Support } from './components/Support/Support';
import './Profile.css';

export function Profile() {
  const { isAuthenticated } = useAuth();

  // Если не авторизован — редирект на главную
  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="profile-page">
      <Sidebar />
      <div className="profile-content">
        <Routes>
          <Route index element={<MainInfo />} />
          <Route path="security" element={<Security />} />
          <Route path="orders" element={<Orders />} />
          <Route path="support" element={<Support />} />
        </Routes>
      </div>
    </div>
  );
}