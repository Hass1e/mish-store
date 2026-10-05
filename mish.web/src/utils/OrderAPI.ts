import { API_BASE_URL, ENDPOINTS } from '../config';

export function getFullImageUrl(imageUrl?: string): string {
  if (!imageUrl) return `${ENDPOINTS.UPLOADS}/placeholder.jpg`;
  
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl;
  }
  
  const path = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;
  return `${API_BASE_URL}${path}`;
}