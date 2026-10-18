import { apiRequest } from './client';
import type { BrandRequest, BrandResponse } from './types';

export const listBrands = () => apiRequest<BrandResponse[]>('/brands');
export const createBrand = (request: BrandRequest) =>
  apiRequest<BrandResponse>('/brands', { method: 'POST', body: JSON.stringify(request) });
export const updateBrand = (id: string, request: BrandRequest) =>
  apiRequest<BrandResponse>(`/brands/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: JSON.stringify(request),
  });
export const deleteBrand = (id: string) =>
  apiRequest<void>(`/brands/${encodeURIComponent(id)}`, { method: 'DELETE' });
