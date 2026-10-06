// categoryApi.ts

const BASE_URL = '/api/categories';

export interface CreateCategoryRequest {
  name: string;
  slug: string;
  parentId?: number | null;
}

export interface CategoryResponse {
  id: number;
  name: string;
  slug: string;
  children: CategoryResponse[];
}

// Вспомогательная функция для обработки ошибок
const handleResponse = async (response: Response) => {
  if (!response.ok) {
    // Пытаемся достать сообщение об ошибке от ASP.NET
    const errorData = await response.json().catch(() => null);
    const message = errorData?.message || errorData?.errors?.Name?.[0] || `Ошибка ${response.status}`;
    throw new Error(message);
  }
  // Если 204 (No Content), возвращаем null, иначе JSON
  if (response.status === 204) return null;
  return response.json();
};

export const categoryApi = {
  // GET /tree - загружаем категории с подкатегориями
  getTree: async (): Promise<CategoryResponse[]> => {
    const response = await fetch(`${BASE_URL}/tree`, {
      method: 'GET',
      credentials: 'include',
    });
    return handleResponse(response);
  },

  // POST / - создание
  create: async (data: CreateCategoryRequest): Promise<CategoryResponse> => {
    const response = await fetch(BASE_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        // Authorization header НЕ нужен, если используются HttpOnly cookies
      },
      body: JSON.stringify(data),
      credentials: 'include',
    });
    return handleResponse(response);
  },
};