import { useState } from 'react';

interface Props {
  orderId: number;
  onClose: () => void;
  onConfirm: () => Promise<boolean>;
}

export function CancelModal({ orderId, onClose, onConfirm }: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleConfirm = async () => {
    setLoading(true);
    setError(null);

    try {
      await onConfirm();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка отмены заказа');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-content--warning" onClick={e => e.stopPropagation()}>
        <h3 className="modal-title modal-title--warning">Отмена заказа #{orderId}</h3>

        <p className="modal-text">
          Вы уверены? Заказ будет отменен. Это действие нельзя отменить.
        </p>

        {error && <div className="modal-error">{error}</div>}

        <div className="modal-actions">
          <button className="modal-btn modal-btn--secondary" onClick={onClose} disabled={loading}>
            Отмена
          </button>
          <button className="modal-btn modal-btn--danger" onClick={handleConfirm} disabled={loading}>
            {loading ? 'Отмена...' : 'Да, отменить'}
          </button>
        </div>
      </div>
    </div>
  );
}