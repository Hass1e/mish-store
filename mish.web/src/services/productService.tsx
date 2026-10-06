// productService.tsx
import { apiFetch } from '../../src/utils/api';
// Базовый URL вашего API. Вынесите его в переменные окружения (.env) в реальном проекте


interface ProductDetailDto {
  [key: string]: unknown;
}

/**
 * Получает детальную информацию о товаре по ID
 * @param {number|string} id - ID товара
 * @returns {Promise<Object>} Данные товара (ProductDetailDto)
 */
export const getProductDetail = async (id: number | string): Promise<ProductDetailDto> => {
  try {
    const response = await apiFetch(`/api/products/${id}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        // Если нужна авторизация, раскомментируйте строку ниже:
        // 'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
    });

    if (!response.ok) {
      // Генерируем понятную ошибку, если бэкенд вернул 404, 500 и т.д.
      throw new Error(`Ошибка сервера: ${response.status} ${response.statusText}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Не удалось загрузить данные товара:', error);
    throw error; // Пробрасываем ошибку дальше, чтобы компонент мог её обработать
  }
};

// productService.js

interface CreatedProductDto {
  [key: string]: unknown;
}

export const createProduct = async (formData: FormData): Promise<CreatedProductDto> => {
  // FormData автоматически установит заголовок Content-Type: multipart/form-data
  const response = await apiFetch('/api/products', {
    method: 'POST',
    body: formData, 
  });

if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    
    // ASP.NET возвращает ошибки в разных полях, проверяем все варианты
    const errorMessage = 
        errorData.message || 
        errorData.title || 
        (typeof errorData.errors === 'object' 
            ? Object.values(errorData.errors).flat().join(', ') 
            : null) ||
        'Ошибка при создании товара';
        
    throw new Error(errorMessage);
}

  return await response.json();
};

export async function getProductForEdit(id: number) {
  const response = await apiFetch(`/api/products/${id}/edit`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${localStorage.getItem('token')}` // Или откуда ты берешь токен
    }
  });

  if (!response.ok) {
    throw new Error('Не удалось загрузить данные товара');
  }

  return await response.json();
}

// mish.web/src/services/productService.ts

export async function updateProduct(id: number, formData: FormData) {
  const token = localStorage.getItem('token'); // Или cookie
  
  const response = await apiFetch(`/api/products/${id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`
      // ⚠️ ВАЖНО: НЕ указывай Content-Type вручную для FormData!
      // Браузер сам выставит правильный boundary
    },
    body: formData
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(errorData?.message || 'Не удалось обновить товар');
  }

  // Для PUT запроса часто возвращается NoContent (204), поэтому json() может упасть
  if (response.status === 204) return true;
  
  return await response.json();
}

export async function getManagedProducts(page: number = 1, size: number = 50) {
  const token = localStorage.getItem('token');
  
  const response = await apiFetch(`/api/products/managed?page=${page}&size=${size}`, {
    method: 'GET',
    headers: { 'Authorization': `Bearer ${token}` }
  });

  if (!response.ok) throw new Error('Не удалось загрузить список товаров');
  return await response.json();
}