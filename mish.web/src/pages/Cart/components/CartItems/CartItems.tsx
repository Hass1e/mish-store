//import { useState, useEffect } from 'react';
//import { apiFetch } from '../../../../utils/api';
import { useCartContext } from '../CartContext.tsx';
import { ENDPOINTS } from '../../../../config';
import './CartItems.css';

export function CartItems() {
  const { cart, removeFromCart, decreaseQuantity, addToCart } = useCartContext();

  if (cart.items.length === 0) {
    return (
      <div className="cart-items">
        <h2 className="cart-items__title">Корзина пуста</h2>
        <p>В корзине пока нет товаров</p>
      </div>
    );
  }

  return (
    <div className="cart-items">
      <h2 className="cart-items__title">
        Корзина ({cart.totalItems} {getItemsWord(cart.totalItems)})
      </h2>

      <div className="cart-items__list">
        {cart.items.map((item) => (
          <CartItemRow
            key={item.productId}
            item={item}
            onRemove={() => removeFromCart(item.productId)}
            onDecrease={() => decreaseQuantity(item.productId)}
            onIncrease={() => addToCart(item.productId, 1)}
          />
        ))}
      </div>
    </div>
  );
}

function CartItemRow({
  item,
  onRemove,
  onDecrease,
  onIncrease,
}: {
  item: any;
  onRemove: () => void;
  onDecrease: () => void;
  onIncrease: () => void;
}) {
  const imageUrl = item.imageUrl
    ? (item.imageUrl.startsWith('http')
        ? item.imageUrl
        : `${ENDPOINTS.UPLOADS}${item.imageUrl.replace('/uploads', '')}`)
    : `${ENDPOINTS.UPLOADS}/placeholder.jpg`;

  return (
    <div className="cart-item">
      <img src={imageUrl} alt={item.productName} className="cart-item__image" />

      <div className="cart-item__info">
        <h3 className="cart-item__title">{item.productName || `Товар #${item.productId}`}</h3>
        <span className="cart-item__price">
          {item.price ? item.price.toLocaleString('ru-RU') : '???'} ₽
        </span>
      </div>

      <div className="cart-item__quantity-controls">
        <button 
          type="button" 
          className="cart-item__qty-btn cart-item__qty-btn--minus"
          onClick={onDecrease}
          aria-label="Уменьшить количество"
        >
          −
        </button>
        
        <span className="cart-item__qty-value">{item.quantity}</span>
        
        <button 
          type="button" 
          className="cart-item__qty-btn cart-item__qty-btn--plus"
          onClick={onIncrease}
          aria-label="Увеличить количество"
        >
          +
        </button>
      </div>

      <button
        type="button"
        className="cart-item__remove"
        onClick={onRemove}
        aria-label="Удалить товар"
      >
        <img src="/trash.png" alt="Удалить" className="cart-item__remove-icon" />
      </button>
    </div>
  );
}

function getItemsWord(count: number): string {
  const lastTwo = count % 100;
  const lastOne = count % 10;
  if (lastTwo >= 11 && lastTwo <= 19) return 'товаров';
  if (lastOne === 1) return 'товар';
  if (lastOne >= 2 && lastOne <= 4) return 'товара';
  return 'товаров';
}