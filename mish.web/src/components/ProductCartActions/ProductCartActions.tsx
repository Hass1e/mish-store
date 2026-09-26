import { useState, type MouseEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCartContext } from '../../pages/Cart/components/CartContext';
import './ProductCartActions.css';

interface ProductCartActionsProps {
  productId: number;
  productName: string;
  price: number;
  imageUrl?: string;
  variant: 'card' | 'detail';
}

export function ProductCartActions({
  productId,
  productName,
  price,
  imageUrl,
  variant
}: ProductCartActionsProps) {
  const navigate = useNavigate();
  const { addToCart, removeFromCart, isInCart } = useCartContext();
  const [isLoading, setIsLoading] = useState(false);
  const isAdded = isInCart(productId);

  const stopEvent = (event: MouseEvent<HTMLButtonElement>) => {
    event.preventDefault();
    event.stopPropagation();
  };

  const handleAdd = async (event: MouseEvent<HTMLButtonElement>) => {
    stopEvent(event);
    if (isLoading) return;

    setIsLoading(true);
    try {
      await addToCart(productId, 1, { productName, price, imageUrl });
    } catch (error) {
      console.error('Ошибка при добавлении товара в корзину:', error);
      window.alert('Не удалось добавить товар в корзину');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemove = async (event: MouseEvent<HTMLButtonElement>) => {
    stopEvent(event);
    if (isLoading) return;

    setIsLoading(true);
    try {
      await removeFromCart(productId);
    } catch (error) {
      console.error('Ошибка при удалении товара из корзины:', error);
      window.alert('Не удалось удалить товар из корзины');
    } finally {
      setIsLoading(false);
    }
  };

  const handleGoToCart = (event: MouseEvent<HTMLButtonElement>) => {
    stopEvent(event);
    navigate('/cart');
  };

  return (
    <div className={`product-cart-actions product-cart-actions--${variant}`}>
      <button
        type="button"
        className={`product-cart-actions__add${isAdded ? ' product-cart-actions__add--added' : ''}`}
        onClick={isAdded ? handleGoToCart : handleAdd}
        disabled={isLoading}
        aria-busy={isLoading}
      >
        {isLoading ? '...' : isAdded ? 'В корзине' : 'В корзину'}
      </button>

      {isAdded && (
        <button
          type="button"
          className="product-cart-actions__remove"
          onClick={handleRemove}
          disabled={isLoading}
          aria-label="Удалить из корзины"
        >
          <img src="/trash.png" alt="" className="product-cart-actions__remove-icon" />
        </button>
      )}
    </div>
  );
}
