// src/config.tsx

// Определяем базовый URL. 
// В разработке это localhost, в продакшене можно будет заменить на http://api.mish.com
export const API_BASE_URL = 'http://localhost:5073'; 

// Экспортируем готовые пути для удобства
export const ENDPOINTS = {
    PRODUCTS: `${API_BASE_URL}/api/products`,
    UPLOADS: `${API_BASE_URL}/uploads`, // Путь к статике
};