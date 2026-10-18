import { apiRequest } from './client';
import type { ProductListParams, ProductListResponse, ProductRequest, ProductResponse } from './types';

export const listProducts = (params: ProductListParams = {}) => {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') query.set(key, String(value));
  }
  return apiRequest<ProductListResponse>(`/products${query.size ? `?${query.toString()}` : ''}`);
};

export const getProduct = (id: string, lang: string) =>
  apiRequest<ProductResponse>(`/products/${encodeURIComponent(id)}?lang=${encodeURIComponent(lang)}`);

export const createProduct = (request: ProductRequest) =>
  apiRequest<ProductResponse>('/products', { method: 'POST', body: JSON.stringify(request) });

export const updateProduct = (id: string, request: ProductRequest) =>
  apiRequest<ProductResponse>(`/products/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  });

export const deleteProduct = (id: string) =>
  apiRequest<void>(`/products/${encodeURIComponent(id)}`, { method: 'DELETE' });

export const downloadProductExcel = () => apiRequest<Blob>('/products/export/download');
