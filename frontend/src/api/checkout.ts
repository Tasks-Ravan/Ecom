import { apiRequest } from './client';
import type { CheckoutRequest, CheckoutResponse } from './types';

export const checkout = (request: CheckoutRequest) =>
  apiRequest<CheckoutResponse>('/checkout', { method: 'POST', body: JSON.stringify(request) });
