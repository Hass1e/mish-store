import { useAdminOrders } from './useAdminOrders';
import { AdminOrderCard } from './AdminOrderCard';
import './AdminOrders.css';

export function AdminOrders() {
  const {
    orders,
    loading,
    hasMore,
    filter,
    setFilter,
    loadMore,
    updateOrderStatus,
    cancelOrder,
    getStatusColor,
    getStatusText,
    getNextStatus,
    formatDate,
  } = useAdminOrders();

  return (
    <div className="admin-orders-container">
      <h2 className="section-title">Управление заказами</h2>

      {/* Фильтры */}
      <div className="admin-filters">
        <input
          type="text"
          placeholder="Поиск по ID заказа или имени клиента..."
          value={filter.search || ''}
          onChange={e => setFilter(prev => ({ ...prev, search: e.target.value, skip: 0 }))}
          className="admin-search-input"
        />

        <select
          value={filter.status || ''}
          onChange={e => setFilter(prev => ({ ...prev, status: e.target.value || undefined, skip: 0 }))}
          className="admin-status-select"
        >
          <option value="">Все статусы</option>
          <option value="Pending">Ожидает</option>
          <option value="Paid">Оплачен</option>
          <option value="Processing">Собирается</option>
          <option value="Shipped">В пути</option>
          <option value="Delivered">Доставлен</option>
          <option value="Cancelled">Отменен</option>
        </select>
      </div>

      {/* Список заказов */}
      {orders.length === 0 && !loading && (
        <div className="empty-state">
          <p>Заказы не найдены</p>
        </div>
      )}

      {orders.map(order => (
        <AdminOrderCard
          key={order.id}
          order={order}
          getStatusColor={getStatusColor}
          getStatusText={getStatusText}
          getNextStatus={getNextStatus}
          formatDate={formatDate}
          onStatusChange={updateOrderStatus}
          onCancel={cancelOrder}
        />
      ))}

      {/* Кнопка "Загрузить еще" */}
      {hasMore && !loading && orders.length > 0 && (
        <button className="load-more-btn" onClick={loadMore}>
          Загрузить еще
        </button>
      )}

      {loading && <div className="loading-spinner">Загрузка...</div>}
    </div>
  );
}