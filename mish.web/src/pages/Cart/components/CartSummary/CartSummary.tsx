import { useState } from 'react';
//import { useNavigate } from 'react-router-dom';
import { useCartContext } from '../CartContext.tsx';
import { useAuth } from '../../../../context/AuthContext';
import { apiFetch } from '../../../../utils/api';
import './CartSummary.css';
//import AuthModal from '../../../../components/AuthModal/AuthModal.tsx'


//function LoginPage() {
 //const navigate = useNavigate()

  //return <AuthModal isOpen={true} onClose={() => navigate('/')} />
//}
export function CartSummary() {
  const { cart } = useCartContext();
  const { isAuthenticated } = useAuth();
  //const navigate = useNavigate();
  const [isProcessing, setIsProcessing] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

// Внутри компонента CartSummary, рядом с другими useState/useEffect

const handleCheckout = async () => {
  if (!isAuthenticated || cart.items.length === 0) return;

  setIsProcessing(true);
  
  try {
    const response = await apiFetch('/api/orders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({}),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => null);
      throw new Error(errorData?.message || `Ошибка сервера: ${response.status}`);
    }

    setIsSuccess(true);
    
    // 🔥 ЖЕСТКАЯ ПЕРЕЗАГРУЗКА С ПЕРЕХОДОМ
    // replace предотвращает возврат на страницу корзины по кнопке "Назад" в браузере
    setTimeout(() => {
      window.location.replace('/profile/orders');
    }, 1500); // Даем 1.5 секунды, чтобы пользователь увидел "Заказ оформлен!"

  } catch (error) {
    console.error('Ошибка оформления заказа:', error);
    alert(error instanceof Error ? error.message : 'Не удалось оформить заказ');
    setIsProcessing(false);
  }
};
  return (
    <div className="cart-summary">
      <h2 className="cart-summary__title">Ваш заказ</h2>

      <div className="cart-summary__row">
        <span className="cart-summary__label">Товары ({cart.totalItems})</span>
        <span className="cart-summary__value">
          {cart.totalPrice.toLocaleString('ru-RU')} ₽
        </span>
      </div>

      <div className="cart-summary__divider" />

      <div className="cart-summary__row cart-summary__row--total">
        <span className="cart-summary__label">Итого</span>
        <span className="cart-summary__value">
          {cart.totalPrice.toLocaleString('ru-RU')} ₽
        </span>
      </div>

      <button
        type="button"
        className={`cart-summary__btn ${!isAuthenticated ? 'cart-summary__btn--login' : ''} ${isSuccess ? 'cart-summary__btn--success' : ''}`}
        onClick={handleCheckout}
        disabled={isProcessing || isSuccess}
      >
        {isProcessing ? 'Оформление...' : isSuccess ? 'Заказ оформлен!' : (!isAuthenticated ? 'Войти и купить' : 'Купить')}
      </button>

      {!isAuthenticated && (
        <p className="cart-summary__note">
          Товары сохранены. <a href="/login">Войдите в аккаунт</a>, чтобы оформить заказ и перенести корзину.
        </p>
      )}
    </div>
  );
}