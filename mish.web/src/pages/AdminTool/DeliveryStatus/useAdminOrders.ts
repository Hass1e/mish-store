import { useState, useEffect, useCallback } from 'react';

export interface AdminOrderItem {
  id: number;
  productId: number;
  productName: string;
  imageUrl?: string;
  price: number;
  quantity: number;
  subtotal: number;
}

export interface AdminOrder {
  id: number;
  userId: number;
  userUsername: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  updatedAt: string;
  items: AdminOrderItem[];
}

export interface AdminOrdersFilter {
  skip: number;
  take: number;
  status?: string;
  search?: string;
}

export function useAdminOrders() {
  const [orders, setOrders] = useState<AdminOrder[]>([]);
  const [loading, setLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [filter, setFilter] = useState<AdminOrdersFilter>({
    skip: 0,
    take: 20,
    status: undefined,
    search: undefined,
  });

  const fetchOrders = useCallback(async (reset = false) => {
    setLoading(true);
    
    try {
      const params = new URLSearchParams({
        skip: reset ? '0' : filter.skip.toString(),
        take: filter.take.toString(),
      });
      
      if (filter.status) params.append('status', filter.status);
      if (filter.search) params.append('search', filter.search);

      const res = await fetch(`/api/orders/admin?${params.toString()}`, {
        credentials: 'include',
      });

      if (!res.ok) {
        throw new Error(`Ошибка загрузки: ${res.status}`);
      }

      const data: AdminOrder[] = await res.json();
      
      // Если пришло меньше записей, чем запрашивали — значит, это конец
      if (data.length < filter.take) {
        setHasMore(false);
      } else {
        setHasMore(true);
      }

      if (reset) {
        setOrders(data);
      } else {
        setOrders(prev => [...prev, ...data]);
      }
    } catch (error) {
      console.error('Ошибка загрузки заказов:', error);
    } finally {
      setLoading(false);
    }
  }, [filter]);

  useEffect(() => {
    fetchOrders(true);
  }, [filter.status, filter.search]);

  const loadMore = () => {
    if (!loading && hasMore) {
      setFilter(prev => ({ ...prev, skip: prev.skip + prev.take }));
    }
  };

  const updateOrderStatus = async (orderId: number, newStatus: string) => {
    try {
      const res = await fetch(`/api/orders/${orderId}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: newStatus }),
        credentials: 'include',
      });

      if (!res.ok) {
        const error = await res.json().catch(() => null);
        throw new Error(error?.message || 'Ошибка изменения статуса');
      }

      // Обновляем заказ в списке
      setOrders(prev =>
        prev.map(order =>
          order.id === orderId
            ? { ...order, status: newStatus, updatedAt: new Date().toISOString() }
            : order
        )
      );

      return true;
    } catch (error) {
      console.error('Ошибка изменения статуса:', error);
      throw error;
    }
  };

  const cancelOrder = async (orderId: number) => {
    return updateOrderStatus(orderId, 'Cancelled');
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Pending': return '#f59e0b';
      case 'Paid': return '#3b82f6';
      case 'Processing': return '#8b5cf6';
      case 'Shipped': return '#0ea5e9';
      case 'Delivered': return '#10b981';
      case 'Cancelled': return '#ef4444';
      default: return '#6b7280';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'Pending': return 'Ожидает';
      case 'Paid': return 'Оплачен';
      case 'Processing': return 'Собирается';
      case 'Shipped': return 'В пути';
      case 'Delivered': return 'Доставлен';
      case 'Cancelled': return 'Отменен';
      default: return status;
    }
  };

  const getNextStatus = (currentStatus: string): { text: string; status: string } | null => {
    switch (currentStatus) {
      case 'Pending': return { text: '→ Оплатить', status: 'Paid' };
      case 'Paid': return { text: '→ В сборку', status: 'Processing' };
      case 'Processing': return { text: '→ Отправить', status: 'Shipped' };
      case 'Shipped': return { text: '→ Доставлен', status: 'Delivered' };
      default: return null;
    }
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return {
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
  };
}
