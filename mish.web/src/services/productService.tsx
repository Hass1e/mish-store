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