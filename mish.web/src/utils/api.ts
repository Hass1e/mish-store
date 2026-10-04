// src/utils/api.ts
import { toast } from 'sonner';

export const apiFetch = async (endpoint: string, options: RequestInit = {}) => {
  const isFormData = options.body instanceof FormData;
  const isURLSearchParams = options.body instanceof URLSearchParams;

  const headers = new Headers(options.headers);
  if (!isFormData && !isURLSearchParams) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(endpoint, {
    ...options,
    credentials: 'include',
    headers,
  });

  if (response.status === 429) {
    toast.warning('Слишком много запросов', {
      id: 'api-rate-limit',
      description: 'Подождите немного и попробуйте снова.',
      duration: 6000,
    });
  }

  return response;
};