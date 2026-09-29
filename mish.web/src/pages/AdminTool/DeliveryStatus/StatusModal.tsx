import { useState } from 'react';
import type { AdminOrder } from './useAdminOrders';

interface Props {
  order: AdminOrder;
  onClose: () => void;
  onConfirm: (newStatus: string) => Promise<boolean>;
  getStatusText: (s: string) => string;
  getStatusColor: (s: string) => string;
}

const ALL_STATUSES = ['Pending', 'Paid', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

export function StatusModal({ order, onClose, onConfirm, getStatusText, getStatusColor }: Props) {
  const [selectedStatus, setSelectedStatus] = useState(order.status);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (selectedStatus === order.status) {
      onClose();
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await onConfirm(selectedStatus);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка изменения статуса');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <h3 className="modal-title">Изменение статуса заказа #{order.id}</h3>

        <div className="modal-field">
          <label className="modal-label">Текущий статус:</label>
          <span 
            className="status-badge" 
            style={{ backgroundColor: getStatusColor(order.status) }}
          >
            {getStatusText(order.status)}
          </span>
        </div>

        <div className="modal-field">
          <label className="modal-label">Новый статус:</label>
          <select
            value={selectedStatus}
            onChange={e => setSelectedStatus(e.target.value)}
            className="modal-select"
          >
            {ALL_STATUSES.map(status => (
              <option key={status} value={status}>
                {getStatusText(status)}
              </option>
            ))}
          </select>
        </div>

        {error && <div className="modal-error">{error}</div>}

        <div className="modal-actions">
          <button className="modal-btn modal-btn--secondary" onClick={onClose} disabled={loading}>
            Отмена
          </button>
          <button className="modal-btn modal-btn--primary" onClick={handleSubmit} disabled={loading}>
            {loading ? 'Сохранение...' : 'Применить'}
          </button>
        </div>
      </div>
    </div>
  );
}