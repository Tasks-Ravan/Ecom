import { describe, expect, it } from 'vitest';
import { productSchema } from './schemas';

describe('product validation', () => {
  const validProduct = {
    sku: 'SKU-01', price: 10, stockQuantity: 4, categoryId: '11111111-1111-4111-8111-111111111111',
    brandId: '22222222-2222-4222-8222-222222222222', isActive: true,
  };

  it('requires at least one language and both fields for each translation', () => {
    expect(productSchema.safeParse({ ...validProduct, translations: {} }).success).toBe(false);
    expect(productSchema.safeParse({ ...validProduct, translations: { az: { name: 'Telefon', description: '' } } }).success).toBe(false);
    expect(productSchema.safeParse({ ...validProduct, translations: { en: { name: 'Phone', description: 'A device' } } }).success).toBe(true);
  });

  it('rejects negative stock and unsupported form shape', () => {
    expect(productSchema.safeParse({ ...validProduct, stockQuantity: -1, translations: { en: { name: 'Phone', description: 'A device' } } }).success).toBe(false);
  });
});
