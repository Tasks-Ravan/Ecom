import { apiRequest } from './client';
import type { AuthResponse, LoginRequest, RegisterRequest, RegisterResponse } from './types';

export const login = (request: LoginRequest) =>
  apiRequest<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify(request) });

export const register = (request: RegisterRequest) =>
  apiRequest<RegisterResponse>('/auth/register', { method: 'POST', body: JSON.stringify(request) });

export const refreshSession = (refreshToken: string) =>
  apiRequest<AuthResponse>('/auth/refresh', { method: 'POST', body: JSON.stringify({ refreshToken }) });
