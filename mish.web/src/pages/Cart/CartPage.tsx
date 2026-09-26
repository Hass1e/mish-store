//mish.web/src/pages/Cart/CartPage.tsx
//import { useCart } from '../../hooks/useCart';
import { CartItems } from './components/CartItems/CartItems';
import { CartSummary } from './components/CartSummary/CartSummary';
import './CartPage.css';
/*
export function CartPage() {
  const { cart, isLoading } = useCart();

  if (isLoading) {
    return (
      <div className="cart-page">
        <div className="cart-loading">Загрузка корзины...</div>
      </div>
    );
  }


  if (cart.items.length === 0) {
    return (
      <div className="cart-page">
        <div className="cart-empty">
          <h2>Корзина пуста</h2>
          <p>Добавьте товары из каталога, чтобы оформить заказ</p>
          <a href="/catalog" className="cart-empty__link">Перейти в каталог</a>
        </div>
      </div>
    );
  }
*/

export function CartPage() {
  return (
    <div className="cart-page">
      <CartItems />
      <CartSummary />
      
    </div>
  );
}