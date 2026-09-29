// mish.web/src/utils/OrderAPI.ts
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5073';

export function getFullImageUrl(imageUrl?: string): string {
  if (!imageUrl) return '/placeholder.png';
  
  // Если уже полный URL (например, с CDN) - возвращаем как есть
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl;
  }
  
  // Относительный путь -> добавляем базовый URL API
  return `${API_BASE_URL}${imageUrl}`;
}