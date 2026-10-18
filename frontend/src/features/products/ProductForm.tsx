import * as Tabs from '@radix-ui/react-tabs';
import { zodResolver } from '@hookform/resolvers/zod';
import { Check, Plus, X } from 'lucide-react';
import { useEffect, useState, type ReactNode } from 'react';
import { useForm } from 'react-hook-form';
import type { Resolver } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import type { BrandResponse, CategoryResponse, Language, ProductRequest, ProductResponse, ProductTranslation } from '../../api/types';
import { listBrands } from '../../api/brands';
import { listCategories } from '../../api/categories';
import { productSchema, type ProductFormValues } from '../../lib/schemas';
import { Input } from '../../components/ui/input';
import { Button } from '../../components/ui/button';
import { LoadingState } from '../../components/States';

const languages: Language[] = ['az', 'en', 'ru'];
const emptyTranslation = (): ProductTranslation => ({ name: '', description: '' });

export function ProductForm({ product, busy, onSave, serverErrors }: {
  product?: ProductResponse & { translations?: Partial<Record<Language, ProductTranslation>> };
  busy: boolean;
  onSave: (request: ProductRequest) => void;
  serverErrors?: Record<string, string[]>;
}) {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<Language>('az');
  const categories = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  const brands = useQuery({ queryKey: ['brands'], queryFn: listBrands });
  const defaults = {
    sku: product?.sku ?? '', price: product?.price ?? 0, stockQuantity: product?.stockQuantity ?? 0,
    categoryId: product?.categoryId ?? '', brandId: product?.brandId ?? '', isActive: product?.isActive ?? true,
    translations: product?.translations ?? {},
  };
  const { register, handleSubmit, setValue, watch, setError, formState: { errors } } = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema) as Resolver<ProductFormValues>, defaultValues: defaults,
  });
  const selectedTranslations = watch('translations');
  useEffect(() => {
    if (serverErrors) {
      for (const [key, messages] of Object.entries(serverErrors)) {
        const normalized = key.toLowerCase();
        const field = normalized.includes('sku') ? 'sku' : normalized.includes('price') ? 'price'
          : normalized.includes('stockquantity') ? 'stockQuantity' : normalized.includes('categoryid') ? 'categoryId'
          : normalized.includes('brandid') ? 'brandId' : null;
        if (field && messages[0]) setError(field, { type: 'server', message: messages[0] });
        const translationMatch = normalized.match(/translations(?:\[(az|en|ru)\]|\.(az|en|ru)).*(name|description)/);
        const translationLanguage = translationMatch?.[1] ?? translationMatch?.[2];
        const translationField = translationMatch?.[3];
        if (translationLanguage && translationField && messages[0]) {
          setError(`translations.${translationLanguage}.${translationField}` as `translations.${Language}.${'name' | 'description'}`, { type: 'server', message: messages[0] });
        } else if (!field && messages[0]) setError('root.server', { type: 'server', message: messages[0] });
      }
    }
  }, [serverErrors, setError]);
  const categoriesWithDepth = categories.data ? flattenCategoryOptions(categories.data) : [];
  const submit = handleSubmit((values) => onSave(values as ProductRequest));

  if (categories.isPending || brands.isPending) return <LoadingState rows={5} />;
  return <form className="product-form form-stack" onSubmit={(event) => void submit(event)} noValidate>
    {errors.root?.server?.message && <p className="form-error" role="alert">{errors.root.server.message}</p>}
    <section className="form-section"><div className="form-section-heading"><div><h2>{t('product.basics')}</h2><p>{t('product.basicsHint')}</p></div></div>
      <div className="grid md:grid-cols-2 gap-5">
        <Field label={t('product.sku')} error={errors.sku?.message}><Input {...register('sku')} maxLength={64} /></Field>
        <Field label={t('common.price')} error={errors.price?.message}><Input type="number" min="0" step="0.01" {...register('price')} /></Field>
        <Field label={t('product.quantity')} error={errors.stockQuantity?.message}><Input type="number" min="0" step="1" {...register('stockQuantity')} /></Field>
        <Field label={t('common.category')} error={errors.categoryId?.message}><select className="input select-native" {...register('categoryId')}><option value="">{t('product.chooseCategory')}</option>
          {categoriesWithDepth.map(({ category, depth }) => <option value={category.id} key={category.id}>{'— '.repeat(depth)}{category.name}</option>)}</select></Field>
        <Field label={t('common.brand')} error={errors.brandId?.message}><select className="input select-native" {...register('brandId')}><option value="">{t('product.chooseBrand')}</option>
          {brands.data?.map((brand: BrandResponse) => <option value={brand.id} key={brand.id}>{brand.name}</option>)}</select></Field>
        <label className="switch-row"><span><strong>{t('product.activeToggle')}</strong><small>{t('product.activeHint')}</small></span>
          <input type="checkbox" role="switch" checked={watch('isActive')} onChange={(event) => setValue('isActive', event.target.checked)} /></label>
      </div>
    </section>
    <section className="form-section"><div className="form-section-heading"><div><h2>{t('product.translations')}</h2><p>{t('product.translationHint')}</p></div>
      <div className="flex gap-2">{languages.filter((language) => !selectedTranslations?.[language]).map((language) =>
        <Button key={language} size="sm" variant="secondary" type="button" onClick={() => { setValue(`translations.${language}`, emptyTranslation()); setActiveTab(language); }}><Plus size={14} />{language.toUpperCase()}</Button>)}</div>
    </div>
      <Tabs.Root value={activeTab} onValueChange={(value) => setActiveTab(value as Language)}>
        <Tabs.List className="translation-tabs" aria-label={t('product.translations')}>
          {languages.filter((language) => Boolean(selectedTranslations?.[language])).map((language) => <Tabs.Trigger className="translation-tab" key={language} value={language}>
            {t(`product.language${language[0].toUpperCase()}${language.slice(1)}`)}{selectedTranslations?.[language]?.name && <Check size={14} className="translation-check" />}</Tabs.Trigger>)}
        </Tabs.List>
        {languages.filter((language) => Boolean(selectedTranslations?.[language])).map((language) => <Tabs.Content value={language} key={language} className="translation-content">
          <div className="flex justify-end"><Button size="sm" variant="ghost" type="button" onClick={() => {
            setValue(`translations.${language}`, undefined);
            const next = languages.find((item) => item !== language && selectedTranslations?.[item]);
            if (next) setActiveTab(next);
          }}><X size={14} />{t('product.removeLanguage')}</Button></div>
          <Field label={t('common.name')} error={errors.translations?.[language]?.name?.message}><Input {...register(`translations.${language}.name`)} maxLength={200} /></Field>
          <Field label={t('common.description')} error={errors.translations?.[language]?.description?.message}><textarea className="input textarea" rows={5} {...register(`translations.${language}.description`)} maxLength={5000} /></Field>
        </Tabs.Content>)}
      </Tabs.Root>
      {errors.translations?.message && <p className="form-error">{errors.translations.message}</p>}
    </section>
    <div className="form-actions"><Button type="submit" disabled={busy || categories.isError || brands.isError}>{busy ? t('common.loading') : product ? t('product.saveProduct') : t('product.createProduct')}</Button></div>
  </form>;
}

function Field({ label, error, children }: { label: string; error?: string; children: ReactNode }) {
  return <label className="form-field"><span className="field-label">{label}</span>{children}{error && <span className="form-error">{error}</span>}</label>;
}

export function flattenCategoryOptions(categories: CategoryResponse[]) {
  const children = new Map<string | null, CategoryResponse[]>();
  categories.forEach((category) => {
    const parent = category.parentCategoryId;
    children.set(parent, [...(children.get(parent) ?? []), category]);
  });
  const result: { category: CategoryResponse; depth: number }[] = [];
  const visit = (parent: string | null, depth: number, ancestors: Set<string>) => {
    (children.get(parent) ?? []).sort((a, b) => a.name.localeCompare(b.name)).forEach((category) => {
      if (ancestors.has(category.id)) return;
      result.push({ category, depth });
      const nextAncestors = new Set(ancestors).add(category.id);
      visit(category.id, depth + 1, nextAncestors);
    });
  };
  visit(null, 0, new Set());
  categories.filter((category) => !result.some((entry) => entry.category.id === category.id)).forEach((category) => result.push({ category, depth: 0 }));
  return result;
}
