import { useState, useEffect } from 'react';

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  imageUrl?: string;
  price: number;
  quantity: number;
  subtotal: number;
}

export interface Order {
  id: number;
  status: string;
  totalAmount: number;
  createdAt: string;
  items: OrderItem[];
}

export function useOrders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedOrderId, setExpandedOrderId] = useState<number | null>(null);

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        // credentials: 'include' обязателен для HttpOnly куки
        const res = await fetch('/api/orders/my', { credentials: 'include' });
        if (res.ok) {
          const data = await res.json();
          setOrders(data);
        }
      } catch (err) {
        console.error('Ошибка загрузки заказов:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, []);

  const toggleOrder = (id: number) => {
    setExpandedOrderId(prev => prev === id ? null : id);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Pending': return '#f59e0b'; // Оранжевый
      case 'Paid': return '#3b82f6';    // Синий
      case 'Processing': return '#8b5cf6'; // Фиолетовый
      case 'Shipped': return '#0ea5e9'; // Голубой
      case 'Delivered': return '#10b981'; // Зеленый
      case 'Cancelled': return '#ef4444'; // Красный
      default: return '#6b7280';
    }
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('ru-RU', {
      day: 'numeric', month: 'long', year: 'numeric'
    });
  };

  return { orders, loading, expandedOrderId, toggleOrder, getStatusColor, formatDate };
}