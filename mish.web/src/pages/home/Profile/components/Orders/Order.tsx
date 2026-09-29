import { useOrders } from './useOrders';
import { OrderCard } from './OrderCard';
import { OrderSkeleton } from './OrderSkeleton';
import './Order.css';

export function Orders() {
  const { orders, loading, expandedOrderId, toggleOrder, getStatusColor, formatDate } = useOrders();

  if (loading) {
    return (
      <div className="orders-container">
        <h2 className="section-title">Мои заказы</h2>
        <OrderSkeleton />
        <OrderSkeleton />
      </div>
    );
  }

  if (orders.length === 0) {
    return (
      <div className="orders-container">
        <h2 className="section-title">Мои заказы</h2>
        <div className="empty-state">
          <p>У вас пока нет заказов</p>
        </div>
      </div>
    );
  }

  return (
    <div className="orders-container">
      <h2 className="section-title">Мои заказы</h2>
      {orders.map(order => (
        <OrderCard
          key={order.id}
          order={order}
          isExpanded={expandedOrderId === order.id}
          onToggle={() => toggleOrder(order.id)}
          getStatusColor={getStatusColor}
          formatDate={formatDate}
        />
      ))}
    </div>
  );
}