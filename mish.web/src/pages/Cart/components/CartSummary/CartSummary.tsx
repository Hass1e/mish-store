import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCartContext } from '../CartContext.tsx';
import { useAuth } from '../../../../context/AuthContext';
import './CartSummary.css';

export function CartSummary() {
  const { cart } = useCartContext();
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [isProcessing, setIsProcessing] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const handleCheckout = async () => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: '/cart' } });
      return;
    }

    setIsProcessing(true);
    try {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      setIsSuccess(true);
      
      setTimeout(() => {
        setIsSuccess(false);
      }, 2000);
    } catch (error) {
      console.error('Ошибка оформления заказа:', error);
      alert('Не удалось оформить заказ');
    } finally {
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