import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ProductForm } from './ProductForm';

vi.mock('../../api/categories', () => ({ listCategories: async () => [] }));
vi.mock('../../api/brands', () => ({ listBrands: async () => [] }));

describe('product translation tabs', () => {
  it('adds and displays a language tab when a translation is selected', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(<QueryClientProvider client={client}><ProductForm busy={false} onSave={vi.fn()} /></QueryClientProvider>);
    fireEvent.click(await screen.findByRole('button', { name: 'AZ' }));
    expect(await screen.findByRole('tab', { name: /Azerbaijani/i })).toBeInTheDocument();
    expect(screen.getByLabelText('Name')).toBeInTheDocument();
  });
});
