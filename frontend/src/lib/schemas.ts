import { z } from 'zod';

export const loginSchema = z.object({
  email: z.email().max(256),
  password: z.string().min(1),
});

export const registerSchema = z.object({
  firstName: z.string().trim().min(1).max(100),
  lastName: z.string().trim().min(1).max(100),
  email: z.email().max(256),
  password: z.string().min(8).regex(/[A-Z]/).regex(/[a-z]/).regex(/[0-9]/).regex(/[^a-zA-Z0-9]/),
});

const translationSchema = z.object({
  name: z.string().trim().min(1).max(200),
  description: z.string().trim().min(1).max(5000),
});

export const productSchema = z.object({
  sku: z.string().trim().min(1).max(64),
  price: z.coerce.number().min(0),
  stockQuantity: z.coerce.number().int().min(0),
  categoryId: z.string().uuid(),
  brandId: z.string().uuid(),
  isActive: z.boolean(),
  translations: z.object({
    az: translationSchema.optional(),
    en: translationSchema.optional(),
    ru: translationSchema.optional(),
  }).refine((translations) => Object.values(translations).some(Boolean), 'Add at least one translation.'),
});

export type ProductFormValues = z.infer<typeof productSchema>;
export type LoginFormValues = z.infer<typeof loginSchema>;
export type RegisterFormValues = z.infer<typeof registerSchema>;
