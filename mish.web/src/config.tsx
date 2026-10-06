// Relative URLs use the Vite proxy in development and the current origin in Docker.
export const API_BASE_URL = '';

// Экспортируем готовые пути для удобства
export const ENDPOINTS = {
    PRODUCTS: `${API_BASE_URL}/api/products`,
    UPLOADS: `${API_BASE_URL}/uploads`, // Путь к статике
};