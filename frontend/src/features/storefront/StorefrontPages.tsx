import { useQuery } from '@tanstack/react-query';
import { ArrowRight, ArrowUpRight, Box, ChevronLeft, ChevronRight, Search, SlidersHorizontal } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { listCategories } from '../../api/categories';
import { listBrands } from '../../api/brands';
import { getProduct, listProducts } from '../../api/products';
import type { ProductResponse } from '../../api/types';
import { ErrorState, LoadingState, EmptyState } from '../../components/States';
import { PageHeader } from '../../components/PageHeader';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { formatCurrency } from '../../lib/utils';
import { useTranslation } from 'react-i18next';

export function HomePage() {
  const { t, i18n } = useTranslation();
  const products = useQuery({ queryKey: ['products', 'home', i18n.language], queryFn: () => listProducts({ page: 1, pageSize: 8, sort: 'created_desc', lang: i18n.language as 'az' | 'en' | 'ru' }) });
  const categories = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  return <>
    <section className="hero page-container">
      <div className="hero-copy"><span className="eyebrow"><span className="eyebrow-dot" />{t('home.eyebrow')}</span>
        <h1>{t('home.title')}</h1><p>{t('home.body')}</p>
        <Link to="/products" className="button button-primary button-default">{t('home.browse')} <ArrowRight size={16} /></Link>
        <div className="hero-note"><span className="avatar-stack"><i /><i /><i /></span><span>{t('home.curated')}</span></div>
      </div>
      <div className="hero-art" aria-hidden="true"><div className="hero-orbit orbit-one" /><div className="hero-orbit orbit-two" />
        <div className="hero-sun" /><div className="hero-product"><Box size={90} strokeWidth={1} /></div>
        <div className="hero-sticker">{t('home.curated')}</div>
      </div>
    </section>
    <section className="page-container section-block">
      <div className="section-heading"><div><span className="eyebrow">{t('home.categories')}</span><h2>{t('home.categories')}</h2></div><Link to="/products" className="text-link">{t('home.browse')} <ArrowUpRight size={15} /></Link></div>
      {categories.isPending ? <LoadingState rows={1} /> : categories.isError ? <ErrorState onRetry={() => void categories.refetch()} /> :
        <div className="category-chips">{categories.data?.filter((category) => category.parentCategoryId === null).map((category) =>
          <Link to={`/products?categoryId=${category.id}`} className="category-chip" key={category.id}><span className="category-chip-mark"><Box size={18} /></span><span>{category.name}</span><ArrowUpRight size={15} /></Link>)}</div>}
    </section>
    <section className="page-container section-block section-bottom">
      <div className="section-heading"><div><span className="eyebrow">{t('home.latest')}</span><h2>{t('home.featured')}</h2><p>{t('home.featuredBody')}</p></div><Link to="/products" className="text-link">{t('nav.products')} <ArrowUpRight size={15} /></Link></div>
      {products.isPending ? <ProductGridSkeleton /> : products.isError ? <ErrorState onRetry={() => void products.refetch()} /> :
        products.data.items.length ? <ProductGrid products={products.data.items} /> : <EmptyState title={t('home.noProducts')} />}
    </section>
  </>;
}

export function ProductsPage() {
  const { t, i18n } = useTranslation();
  const [params, setParams] = useSearchParams();
  const [searchInput, setSearchInput] = useState(params.get('search') ?? '');
  const [minPrice, setMinPrice] = useState(params.get('minPrice') ?? '');
  const [maxPrice, setMaxPrice] = useState(params.get('maxPrice') ?? '');
  const page = Number(params.get('page') || 1);
  const query = useQuery({
    queryKey: ['products', 'list', params.toString(), i18n.language],
    queryFn: () => listProducts({ search: params.get('search') || undefined, categoryId: params.get('categoryId') || undefined,
      brandId: params.get('brandId') || undefined, minPrice: params.get('minPrice') ? Number(params.get('minPrice')) : undefined,
      maxPrice: params.get('maxPrice') ? Number(params.get('maxPrice')) : undefined, page, pageSize: Number(params.get('pageSize') || 12),
      sort: (params.get('sort') as 'price_asc' | 'price_desc' | 'created_desc') || 'created_desc', lang: i18n.language as 'az' | 'en' | 'ru' }),
  });
  const categories = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  const brands = useQuery({ queryKey: ['brands'], queryFn: listBrands });
  useEffect(() => {
    const timeout = window.setTimeout(() => updateParam(setParams, 'search', searchInput), 300);
    return () => window.clearTimeout(timeout);
  }, [searchInput, setParams]);
  useEffect(() => {
    const timeout = window.setTimeout(() => setParams((current) => {
      const next = new URLSearchParams(current);
      if (minPrice) next.set('minPrice', minPrice); else next.delete('minPrice');
      if (maxPrice) next.set('maxPrice', maxPrice); else next.delete('maxPrice');
      next.delete('page');
      return next;
    }, { replace: true }), 250);
    return () => window.clearTimeout(timeout);
  }, [minPrice, maxPrice, setParams]);
  const setFilter = (key: string, value: string) => updateParam(setParams, key, value);
  return <section className="page-container content-page">
    <PageHeader title={t('product.title')} description={t('product.description')} />
    <div className="catalog-layout">
      <aside className="filter-panel"><div className="filter-title"><SlidersHorizontal size={17} /><strong>{t('product.filters')}</strong></div>
        <label className="form-field"><span className="field-label">{t('common.search')}</span><span className="field-with-icon"><span className="field-icon"><Search size={16} /></span><Input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder={t('product.searchPlaceholder')} /></span></label>
        <label className="form-field"><span className="field-label">{t('common.category')}</span><select className="input select-native" value={params.get('categoryId') ?? ''} onChange={(event) => setFilter('categoryId', event.target.value)}><option value="">{t('common.all')}</option>{categories.data?.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
        <label className="form-field"><span className="field-label">{t('common.brand')}</span><select className="input select-native" value={params.get('brandId') ?? ''} onChange={(event) => setFilter('brandId', event.target.value)}><option value="">{t('common.all')}</option>{brands.data?.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}</select></label>
        <div className="form-field"><span className="field-label">{t('product.filters')}</span><div className="grid grid-cols-2 gap-2"><Input type="number" min="0" aria-label={t('product.minPrice')} placeholder={t('product.minPrice')} value={minPrice} onChange={(event) => setMinPrice(event.target.value)} /><Input type="number" min="0" aria-label={t('product.maxPrice')} placeholder={t('product.maxPrice')} value={maxPrice} onChange={(event) => setMaxPrice(event.target.value)} /></div></div>
        <Button variant="ghost" size="sm" onClick={() => { setSearchInput(''); setMinPrice(''); setMaxPrice(''); setParams({}); }}>{t('product.clearFilters')}</Button>
      </aside>
      <div className="catalog-results"><div className="catalog-toolbar"><span className="muted">{query.data?.totalCount ?? 0} {t('product.title').toLocaleLowerCase(i18n.language)}</span>
        <label className="sort-control"><span>{t('product.sort')}</span><select className="input select-native" value={params.get('sort') || 'created_desc'} onChange={(event) => setFilter('sort', event.target.value)}>
          <option value="created_desc">{t('product.newest')}</option><option value="price_asc">{t('product.priceLow')}</option><option value="price_desc">{t('product.priceHigh')}</option>
        </select></label></div>
        {query.isPending ? <ProductGridSkeleton /> : query.isError ? <ErrorState onRetry={() => void query.refetch()} /> : query.data.items.length ? <ProductGrid products={query.data.items} /> : <EmptyState title={t('product.noProducts')} />}
        {query.data && query.data.totalCount > query.data.pageSize && <div className="pagination-bar"><Button variant="secondary" size="sm" disabled={page <= 1} onClick={() => setFilter('page', String(page - 1))}><ChevronLeft size={15} />{t('common.previous')}</Button>
          <span>{t('common.page')} {page} {t('common.of')} {Math.ceil(query.data.totalCount / query.data.pageSize)}</span>
          <Button variant="secondary" size="sm" disabled={page * query.data.pageSize >= query.data.totalCount} onClick={() => setFilter('page', String(page + 1))}>{t('common.next')}<ChevronRight size={15} /></Button></div>}
      </div>
    </div>
  </section>;
}

export function ProductDetailPage() {
  const { t, i18n } = useTranslation();
  const { id = '' } = useParams();
  const query = useQuery({ queryKey: ['product', id, i18n.language], queryFn: () => getProduct(id, i18n.language), enabled: Boolean(id) });
  if (query.isPending) return <section className="page-container content-page"><LoadingState rows={2} /></section>;
  if (query.isError) return <section className="page-container content-page"><ErrorState message={t('errors.notFound')} onRetry={() => void query.refetch()} /></section>;
  const product = query.data;
  return <section className="page-container content-page"><div className="breadcrumbs detail-crumbs"><Link to="/">{t('product.breadcrumbHome')}</Link><ChevronRight size={14} /><Link to={`/products?categoryId=${product.categoryId}`}>{product.categoryName}</Link><ChevronRight size={14} /><span>{product.name}</span></div>
    <div className="product-detail-grid"><div className="product-detail-art"><div className="product-art-symbol"><Box size={96} strokeWidth={1} /></div><span className="product-art-label">{product.sku}</span></div>
      <div className="product-detail-copy"><Badge className="category-badge">{product.categoryName}</Badge><h1>{product.name}</h1><p className="detail-brand">{product.brandName}</p>
        <div className="detail-price">{formatCurrency(product.price, i18n.language)}</div><p className="detail-description">{product.description}</p>
        <div className="detail-meta"><span>{t('product.sku')}</span><strong>{product.sku}</strong></div>
        <div className="detail-meta"><span>{t('common.stock')}</span><Badge className={product.stockQuantity > 0 ? 'status-active' : 'status-inactive'}>{product.stockQuantity > 0 ? t('product.inStock') : t('product.outOfStock')}</Badge></div>
        <div className="detail-meta"><span>{t('common.category')}</span><strong>{product.categoryName}</strong></div><div className="detail-meta"><span>{t('common.brand')}</span><strong>{product.brandName}</strong></div>
      </div></div></section>;
}

export function ProductGrid({ products }: { products: ProductResponse[] }) {
  const { t, i18n } = useTranslation();
  return <div className="product-grid">{products.map((product) => <Link className="product-card" key={product.id} to={`/products/${product.id}`}>
    <div className="product-card-art"><div className="product-art-symbol"><Box size={48} strokeWidth={1.25} /></div><span className="product-art-label">{product.sku}</span></div>
    <div className="product-card-copy"><div className="product-card-categories"><span>{product.categoryName}</span><span>{product.brandName}</span></div>
      <h3>{product.name}</h3><div className="product-card-bottom"><strong>{formatCurrency(product.price, i18n.language)}</strong>
        <Badge className={product.stockQuantity > 0 ? 'stock-dot in-stock' : 'stock-dot out-stock'}>{product.stockQuantity > 0 ? t('product.inStock') : t('product.outOfStock')}</Badge></div></div>
  </Link>)}</div>;
}

export function ProductGridSkeleton() { return <div className="product-grid">{Array.from({ length: 4 }, (_, index) => <div className="product-card" key={index}><div className="skeleton aspect-square" /><div className="p-4 space-y-3"><div className="skeleton h-3 w-1/3" /><div className="skeleton h-5 w-3/4" /><div className="skeleton h-4 w-1/2" /></div></div>)}</div>; }

function updateParam(setParams: (next: URLSearchParams | ((current: URLSearchParams) => URLSearchParams), options?: { replace?: boolean }) => void, key: string, value: string) {
  setParams((current) => {
    const next = new URLSearchParams(current);
    if (value) next.set(key, value); else next.delete(key);
    if (key !== 'page') next.delete('page');
    return next;
  }, { replace: true });
}
