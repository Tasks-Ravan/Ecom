export type Language = 'az' | 'en' | 'ru';
export type UserRole = 'Admin' | 'Customer';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface RegisterResponse {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface ProductTranslation {
  name: string;
  description: string;
}

export interface ProductRequest {
  sku: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
  brandId: string;
  isActive: boolean;
  translations: Partial<Record<Language, ProductTranslation>>;
}

export interface ProductResponse {
  id: string;
  sku: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
  categoryName: string;
  brandId: string;
  brandName: string;
  isActive: boolean;
  language: string;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export interface CheckoutItemRequest {
  productId: string;
  quantity: number;
}

export interface CheckoutRequest {
  items: CheckoutItemRequest[];
  simulateDecline: boolean;
  language: Language;
}

export interface CheckoutResponse {
  orderId: string;
  orderNumber: string;
  status: 'Paid' | 'PaymentDeclined';
  paymentProvider: 'Sandbox';
  totalAmount: number;
  currency: 'AZN';
  message: string;
  createdAt: string;
}

export interface ProductListResponse {
  items: ProductResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ProductListParams {
  search?: string;
  categoryId?: string;
  brandId?: string;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
  sort?: 'price_asc' | 'price_desc' | 'created_desc';
  lang?: Language;
}

export interface CategoryRequest {
  name: string;
  parentCategoryId: string | null;
  isActive: boolean;
}

export interface CategoryResponse {
  id: string;
  name: string;
  slug: string;
  parentCategoryId: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface BrandRequest {
  name: string;
  logoUrl: string | null;
  isActive: boolean;
}

export interface BrandResponse {
  id: string;
  name: string;
  slug: string;
  logoUrl: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ExportProductsToMailRequest {
  recipientEmail: string;
}

export interface ExportProductsToMailResponse {
  jobId: string;
  status: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  error?: string;
}
