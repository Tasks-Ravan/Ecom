import { createBrowserRouter } from 'react-router-dom';
import { AdminLayout } from '../layouts/AdminLayout';
import { AuthLayout, StorefrontLayout } from '../layouts/StorefrontLayout';
import { GoogleOAuthCallbackPage, LoginPage, RegisterPage } from '../features/auth/AuthPages';
import { HomePage, ProductDetailPage, ProductsPage } from '../features/storefront/StorefrontPages';
import { AdminProductsPage, ProductEditorPage } from '../features/products/AdminProductsPage';
import { CategoriesPage } from '../features/categories/CategoriesPage';
import { BrandsPage } from '../features/brands/BrandsPage';
import { ExportsPage } from '../features/exports/ExportsPage';
import { DashboardPage } from '../features/dashboard/DashboardPage';
import { CartPage, CheckoutPage } from '../features/checkout/CheckoutPages';
import { AdminRoute, ForbiddenPage, NotFoundPage } from './guards';

export const router = createBrowserRouter([
  { element: <StorefrontLayout />, children: [
    { path: '/', element: <HomePage /> },
    { path: '/products', element: <ProductsPage /> },
    { path: '/products/:id', element: <ProductDetailPage /> },
    { path: '/cart', element: <CartPage /> },
    { path: '/checkout', element: <CheckoutPage /> },
  ] },
  { element: <AuthLayout />, children: [
    { path: '/login', element: <LoginPage /> },
    { path: '/register', element: <RegisterPage /> },
    { path: '/auth/google/callback', element: <GoogleOAuthCallbackPage /> },
  ] },
  { element: <AdminRoute />, children: [{ path: '/admin', element: <AdminLayout />, children: [
    { index: true, element: <DashboardPage /> },
    { path: 'products', element: <AdminProductsPage /> },
    { path: 'products/new', element: <ProductEditorPage /> },
    { path: 'products/:id/edit', element: <ProductEditorPage /> },
    { path: 'categories', element: <CategoriesPage /> },
    { path: 'brands', element: <BrandsPage /> },
    { path: 'exports', element: <ExportsPage /> },
  ] }] },
  { path: '/403', element: <ForbiddenPage /> },
  { path: '*', element: <NotFoundPage /> },
]);
