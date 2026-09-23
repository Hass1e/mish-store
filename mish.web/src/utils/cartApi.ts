// src/utils/cartApi.ts
import { apiFetch } from './api';

export const cartApi = {
  getCart: async () => {
    const response = await apiFetch('/api/cart');
    if (!response.ok) throw new Error('Не удалось загрузить корзину');
    return response.json();
  },

  addToCart: async (productId: number, quantity: number = 1) => {
    const response = await apiFetch('/api/cart/add', {
      method: 'POST',
      body: JSON.stringify({ productId, quantity }),
    });
    if (!response.ok) throw new Error('Не удалось добавить товар');
    return response.json();
  },

  removeFromCart: async (productId: number) => {
    const response = await apiFetch(`/api/cart/remove-by-product/${productId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Не удалось удалить товар');
  }
};