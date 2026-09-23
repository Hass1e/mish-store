import './styles/header.css'

import { useState } from 'react'
import { Link } from 'react-router-dom'
import { CatalogMenu } from './components/CatalogMenu/CatalogMenu'
import AuthModal from './components/AuthModal/AuthModal.tsx'
import { useAuth } from './context/AuthContext'; // Путь к твоему контексту






export function Header() {
    const [isCatalogOpen, setIsCatalogOpen] = useState(false)
    const { isAuthenticated, user, logout } = useAuth();
    const [isModalOpen, setIsModalOpen] = useState(false)

    return (
        <header className="header">
            <div className="header__container">
                <Link to="/" className="header__logo">
                    M I S H
                </Link>

                <div className="header__actions">
                    {/* Кнопка Каталог */}
                    <button 
                        className="header__action-btn header__action-btn--catalog"
                        onClick={() => setIsCatalogOpen(true)}
                    >
                        Каталог
                    </button>
                    
                <div className="header__actions">
                    {isAuthenticated && user ? (
                    // ✅ АВТОРИЗОВАННЫЙ ПОЛЬЗОВАТЕЛЬ
                    <div className="header__user-menu" style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
                        <Link to="/profile" className="header__action-btn header__action-btn--profile">
                        Профиль
                        </Link>
                        <button 
                        onClick={logout} 
                        className="header__action-btn header__action-btn--logout"
                        >
                        Выйти
                        </button>
                    </div>
                    ) : (
                    // ✅ ГОСТЬ
                    <>
                        <button 
                        className="header__action-btn" 
                        onClick={() => setIsModalOpen(true)}
                        >
                        Вход
                        </button>
                        <AuthModal 
                        isOpen={isModalOpen} 
                        onClose={() => setIsModalOpen(false)} 
                        />
                    </>
                    )}
                </div>

                     <Link to="/cart" className="header__action-btn" >  Корзина  </Link> 
                </div>
            </div>

            {/* Меню рендерится поверх всего */}
            <CatalogMenu 
                isOpen={isCatalogOpen} 
                onClose={() => setIsCatalogOpen(false)} 
            />
        </header>
    )
}