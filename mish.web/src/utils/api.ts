// src/utils/api.ts

export const apiFetch = async (endpoint: string, options: RequestInit = {}) => {
  const isFormData = options.body instanceof FormData;
  const isURLSearchParams = options.body instanceof URLSearchParams;

  // Создаем новый объект заголовков, чтобы не мутировать оригинал
  const headers = new Headers(options.headers);

  // Добавляем JSON только если это НЕ форма и НЕ поисковые параметры
  if (!isFormData && !isURLSearchParams) {
    headers.set('Content-Type', 'application/json');
  }

  return fetch(endpoint, {
    ...options,
    credentials: 'include',
    headers, // Передаем объект Headers напрямую
  });
};