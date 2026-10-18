import { apiRequest } from './client';
import type { CategoryRequest, CategoryResponse } from './types';

export const listCategories = () => apiRequest<CategoryResponse[]>('/categories');
export const createCategory = (request: CategoryRequest) =>
  apiRequest<CategoryResponse>('/categories', { method: 'POST', body: JSON.stringify(request) });
export const updateCategory = (id: string, request: CategoryRequest) =>
  apiRequest<CategoryResponse>(`/categories/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  });
export const deleteCategory = (id: string) =>
  apiRequest<void>(`/categories/${encodeURIComponent(id)}`, { method: 'DELETE' });
