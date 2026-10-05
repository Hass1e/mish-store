// src/utils/cartApi.ts
import { apiFetch } from './api';

export const cartApi = {
  getCart: async () => {
    const response = await apiFetch('/api/cart');
    if (!response.ok) throw await createCartError(response, 'Не удалось загрузить корзину');
    return response.json();
  },

  addToCart: async (productId: number, quantity: number = 1) => {
    const response = await apiFetch('/api/cart/add', {
      method: 'POST',
      body: JSON.stringify({ productId, quantity }),
    });
    if (!response.ok) throw await createCartError(response, 'Не удалось добавить товар');
    return response.json();
  },

  decreaseQuantity: async (productId: number) => {
    const response = await apiFetch(`/api/cart/decrease/${productId}`, {
      method: 'POST',
    });
    if (!response.ok) throw await createCartError(response, 'Не удалось изменить количество товара');
    return response.json();
  },

  removeFromCart: async (productId: number) => {
    const response = await apiFetch(`/api/cart/remove-by-product/${productId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw await createCartError(response, 'Не удалось удалить товар');
  }
};

async function createCartError(response: Response, fallback: string): Promise<Error> {
  const error = await response.json().catch(() => null);
  return new Error(error?.message || error?.title || `${fallback} (${response.status})`);
}