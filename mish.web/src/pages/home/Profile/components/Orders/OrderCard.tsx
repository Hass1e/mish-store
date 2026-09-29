import { Link } from 'react-router-dom';
import type { Order } from './useOrders';
import { getFullImageUrl } from '../../../../../utils/OrderAPI';

interface Props {
  order: Order;
  isExpanded: boolean;
  onToggle: () => void;
  getStatusColor: (s: string) => string;
  formatDate: (d: string) => string;
}

export function OrderCard({ order, isExpanded, onToggle, getStatusColor, formatDate }: Props) {
  // Уникальные товары для миниатюр в шапке
  const uniqueItems = order.items.filter(
    (item, idx, arr) => arr.findIndex(i => i.productId === item.productId) === idx
  );

  return (
    <div className={`order-card ${isExpanded ? 'expanded' : ''}`}>
      {/* Шапка заказа */}
      {/* Шапка заказа */}
<div className="order-header" onClick={onToggle}>
  
  {/* ЛЕВАЯ ЧАСТЬ: Инфо и Миниатюры (занимает всё доступное место) */}
  <div className="order-left">
    <div className="order-info">
      <span className="order-id">Заказ №{order.id}</span>
      <span className="order-date">{formatDate(order.createdAt)}</span>
    </div>
    
    {/* Миниатюры пропадают при раскрытии, но это не сдвинет правую часть */}
    {!isExpanded && (
      <div className="order-thumbs">
        {uniqueItems.map(item => (
          <Link 
            key={item.productId} 
            to={`/product/${item.productId}`}
            className="thumb-link"
            onClick={(e) => e.stopPropagation()}
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

  {/* ПРАВАЯ ЧАСТЬ: Статус, Цена и Стрелка (жестко прижата вправо) */}
  <div className="order-right">
    <span 
      className="status-badge" 
      style={{ backgroundColor: getStatusColor(order.status) }}
    >
      {order.status === 'Pending' && 'Ожидает'}
      {order.status === 'Paid' && 'Оплачен'}
      {order.status === 'Processing' && 'Собирается'}
      {order.status === 'Shipped' && 'В пути'}
      {order.status === 'Delivered' && 'Доставлен'}
      {order.status === 'Cancelled' && 'Отменен'}
    </span>

    <div className="order-total">
      {order.totalAmount.toLocaleString('ru-RU')} ₽
    </div>

    <button className="toggle-btn">
      {isExpanded ? '▲' : '▼'}
    </button>
  </div>

</div>

      {/* Развернутый список товаров */}
      {isExpanded && (
        <div className="order-details">
          {order.items.map((item, index) => (
            <Link 
              key={item.id} 
              to={`/product/${item.productId}`}
              className="order-item-row"
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
      )}
    </div>
  );
}