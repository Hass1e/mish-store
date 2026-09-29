import { useState } from 'react';
import { Link } from 'react-router-dom';
import type { AdminOrder } from './useAdminOrders';
import { StatusModal } from './StatusModal';
import { CancelModal } from './CancelModal';
import { getFullImageUrl } from '../../../utils/OrderAPI';


interface Props {
  order: AdminOrder;
  getStatusColor: (s: string) => string;
  getStatusText: (s: string) => string;
  getNextStatus: (s: string) => { text: string; status: string } | null;
  formatDate: (d: string) => string;
  onStatusChange: (orderId: number, newStatus: string) => Promise<boolean>; 
  onCancel: (orderId: number) => Promise<boolean>; 
}

export function AdminOrderCard({
  order,
  getStatusColor,
  getStatusText,
  getNextStatus,
  formatDate,
  onStatusChange,
  onCancel,
}: Props) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);

  const nextStatus = getNextStatus(order.status);
  const isFinalStatus = order.status === 'Delivered' || order.status === 'Cancelled';

  const uniqueItems = order.items.filter(
    (item, idx, arr) => arr.findIndex(i => i.productId === item.productId) === idx
  );

  return (
    <>
      <div className={`admin-order-card ${isExpanded ? 'expanded' : ''}`}>
        {/* Шапка */}
        <div className="admin-order-header" onClick={() => setIsExpanded(!isExpanded)}>
          <div className="admin-order-left">
            <div className="admin-order-info">
              <span className="admin-order-id">Заказ #{order.id}</span>
              <span className="admin-order-client">Клиент: {order.userUsername} (ID: {order.userId})</span>
            </div>

            {!isExpanded && (
              <div className="admin-order-thumbs">
                {uniqueItems.slice(0, 5).map(item => (
                  <Link
                    key={item.productId}
                    to={`/product/${item.productId}`}
                    className="thumb-link"
                    onClick={e => e.stopPropagation()}
                  >
                    <img
                      src={getFullImageUrl(item.imageUrl)}
                      alt={item.productName}
                      className="thumb-img"
                    />
                  </Link>
                ))}
              </div>
            )}
          </div>

          <div className="admin-order-right">
            <span
              className="status-badge"
              style={{ backgroundColor: getStatusColor(order.status) }}
            >
              {getStatusText(order.status)}
            </span>

            <div className="admin-order-total">
              {order.totalAmount.toLocaleString('ru-RU')} ₽
            </div>

            <span className="admin-order-date">{formatDate(order.createdAt)}</span>

            <button className="toggle-btn">
              {isExpanded ? '▲' : '▼'}
            </button>
          </div>
        </div>

        {/* Развернутая часть */}
        {isExpanded && (
          <div className="admin-order-details">
            <div className="admin-order-section">
              <h4 className="admin-order-section-title">Состав заказа</h4>
              {order.items.map((item, index) => (
                <Link
                  key={item.id}
                  to={`/product/${item.productId}`}
                  className="admin-order-item-row"
                >
                  <span className="item-number">{index + 1}</span>
                  <img
                    src={getFullImageUrl(item.imageUrl)}
                    alt={item.productName}
                    className="item-img"
                  />
                  <div className="item-info">
                    <span className="item-name">{item.productName}</span>
                    <span className="item-calc">
                      {item.quantity} шт. × {item.price.toLocaleString('ru-RU')} ₽ = {item.subtotal.toLocaleString('ru-RU')} ₽
                    </span>
                  </div>
                </Link>
              ))}
            </div>

            <div className="admin-order-section">
              <h4 className="admin-order-section-title">Информация</h4>
              <div className="admin-order-dates">
                <div className="admin-order-date-row">
                  <span className="admin-order-date-label">Создан:</span>
                  <span className="admin-order-date-value">{formatDate(order.createdAt)}</span>
                </div>
                <div className="admin-order-date-row">
                  <span className="admin-order-date-label">Обновлен:</span>
                  <span className="admin-order-date-value">{formatDate(order.updatedAt)}</span>
                </div>
              </div>
            </div>

            <div className="admin-order-actions">
              {nextStatus && (
                <button
                  className="admin-btn admin-btn--primary"
                  onClick={async (e) => {
                    e.stopPropagation();
                    await onStatusChange(order.id, nextStatus.status);
                  }}
                >
                  {nextStatus.text}
                </button>
              )}

              <button
                className="admin-btn admin-btn--secondary"
                onClick={e => {
                  e.stopPropagation();
                  setShowStatusModal(true);
                }}
              >
                ⚙ Изменить статус
              </button>

              {!isFinalStatus && (
                <button
                  className="admin-btn admin-btn--danger"
                  onClick={e => {
                    e.stopPropagation();
                    setShowCancelModal(true);
                  }}
                >
                  ✕ Отменить заказ
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {showStatusModal && (
        <StatusModal
          order={order}
          onClose={() => setShowStatusModal(false)}
          onConfirm={newStatus => onStatusChange(order.id, newStatus)}
          getStatusText={getStatusText}
          getStatusColor={getStatusColor}
        />
      )}

      {showCancelModal && (
        <CancelModal
          orderId={order.id}
          onClose={() => setShowCancelModal(false)}
          onConfirm={() => onCancel(order.id)}
        />
      )}
    </>
  );
}