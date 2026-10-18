import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createColumnHelper, flexRender, getCoreRowModel, useReactTable } from '@tanstack/react-table';
import { Eye, Pencil, Plus, Trash2 } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { toast } from 'sonner';
import { listBrands } from '../../api/brands';
import { listCategories } from '../../api/categories';
import { ApiError } from '../../api/client';
import { createProduct, deleteProduct, getProduct, listProducts, updateProduct } from '../../api/products';
import type { Language, ProductRequest, ProductResponse, ProductTranslation } from '../../api/types';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { ErrorState, LoadingState, EmptyState } from '../../components/States';
import { PageHeader } from '../../components/PageHeader';
import { Badge } from '../../components/ui/badge';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { useTranslation } from 'react-i18next';
import { formatCurrency } from '../../lib/utils';
import { ProductForm } from './ProductForm';

const productColumn = createColumnHelper<ProductResponse>();

export function AdminProductsPage() {
  const { t, i18n } = useTranslation();
  const [params, setParams] = useSearchParams();
  const [searchValue, setSearchValue] = useState(params.get('search') ?? '');
  const [deleteTarget, setDeleteTarget] = useState<ProductResponse | null>(null);
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const page = Number(params.get('page') ?? 1);
  const pageSize = Number(params.get('pageSize') ?? 12);
  const query = useQuery({ queryKey: ['products', 'admin', params.toString(), i18n.language], queryFn: () => listProducts({
    search: params.get('search') || undefined, categoryId: params.get('categoryId') || undefined,
    brandId: params.get('brandId') || undefined, minPrice: params.has('minPrice') ? Number(params.get('minPrice')) : undefined,
    maxPrice: params.has('maxPrice') ? Number(params.get('maxPrice')) : undefined, page, pageSize,
    sort: (params.get('sort') as 'price_asc' | 'price_desc' | 'created_desc') || 'created_desc', lang: i18n.language as Language,
  }) });
  const categories = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  const brands = useQuery({ queryKey: ['brands'], queryFn: listBrands });
  useEffect(() => {
    const timer = window.setTimeout(() => setParam(setParams, 'search', searchValue), 300);
    return () => window.clearTimeout(timer);
  }, [searchValue, setParams]);
  const remove = useMutation({ mutationFn: deleteProduct, onSuccess: async () => {
    toast.success(t('common.deleted')); setDeleteTarget(null); await queryClient.invalidateQueries({ queryKey: ['products'] });
  }, onError: (error) => toast.error(error instanceof ApiError ? error.message : t('errors.request')) });

  const columns = useMemo(() => [
    productColumn.accessor('name', { header: t('common.name'), cell: (info) => <div className="table-product"><span className="table-product-mark">P</span><span><strong>{info.getValue()}</strong><small>{info.row.original.sku}</small></span></div> }),
    productColumn.accessor('categoryName', { header: t('common.category') }),
    productColumn.accessor('brandName', { header: t('common.brand') }),
    productColumn.accessor('price', { header: t('common.price'), cell: (info) => <strong>{formatCurrency(info.getValue(), i18n.language)}</strong> }),
    productColumn.accessor('stockQuantity', { header: t('common.stock'), cell: (info) => <span className={info.getValue() <= 5 ? 'stock-low' : ''}>{info.getValue()}</span> }),
    productColumn.accessor('isActive', { header: t('common.status'), cell: (info) => <Badge className={info.getValue() ? 'status-active' : 'status-inactive'}>{info.getValue() ? t('common.active') : t('common.inactive')}</Badge> }),
    productColumn.display({ id: 'actions', header: t('common.actions'), cell: (info) => <div className="table-actions">
      <Link className="icon-button" aria-label={t('common.view')} to={`/products/${info.row.original.id}`}><Eye size={16} /></Link>
      <Link className="icon-button" aria-label={t('common.edit')} to={`/admin/products/${info.row.original.id}/edit`}><Pencil size={16} /></Link>
      <button className="icon-button danger-icon" aria-label={t('common.delete')} onClick={() => setDeleteTarget(info.row.original)}><Trash2 size={16} /></button>
    </div> }),
  ], [t, i18n.language]);
  const table = useReactTable({ data: query.data?.items ?? [], columns, getCoreRowModel: getCoreRowModel() });

  return <>
    <PageHeader title={t('admin.products')} description={t('admin.productsDescription')} action={<Button onClick={() => navigate('/admin/products/new')}><Plus size={16} />{t('product.createProduct')}</Button>} />
    <section className="card admin-table-card">
      <div className="table-toolbar"><label className="search-control"><Input value={searchValue} onChange={(event) => setSearchValue(event.target.value)} placeholder={t('product.searchPlaceholder')} aria-label={t('common.search')} /></label>
        <select className="input select-native filter-select" aria-label={t('common.category')} value={params.get('categoryId') ?? ''} onChange={(event) => setParam(setParams, 'categoryId', event.target.value)}><option value="">{t('common.category')}: {t('common.all')}</option>{categories.data?.map((item) => <option value={item.id} key={item.id}>{item.name}</option>)}</select>
        <select className="input select-native filter-select" aria-label={t('common.brand')} value={params.get('brandId') ?? ''} onChange={(event) => setParam(setParams, 'brandId', event.target.value)}><option value="">{t('common.brand')}: {t('common.all')}</option>{brands.data?.map((item) => <option value={item.id} key={item.id}>{item.name}</option>)}</select>
        <Input className="price-filter" aria-label={t('product.minPrice')} type="number" min="0" placeholder={t('product.minPrice')} value={params.get('minPrice') ?? ''} onChange={(event) => setParam(setParams, 'minPrice', event.target.value)} />
        <Input className="price-filter" aria-label={t('product.maxPrice')} type="number" min="0" placeholder={t('product.maxPrice')} value={params.get('maxPrice') ?? ''} onChange={(event) => setParam(setParams, 'maxPrice', event.target.value)} />
        <select className="input select-native filter-select" aria-label={t('product.sort')} value={params.get('sort') || 'created_desc'} onChange={(event) => setParam(setParams, 'sort', event.target.value)}>
          <option value="created_desc">{t('product.newest')}</option><option value="price_asc">{t('product.priceLow')}</option><option value="price_desc">{t('product.priceHigh')}</option></select>
      </div>
      {query.isPending ? <div className="p-5"><LoadingState /></div> : query.isError ? <div className="p-5"><ErrorState onRetry={() => void query.refetch()} /></div> : query.data.items.length === 0 ? <EmptyState title={t('product.noProducts')} action={<Button variant="secondary" onClick={() => navigate('/admin/products/new')}><Plus size={15} />{t('product.createProduct')}</Button>} /> :
        <div className="table-scroll"><table className="data-table"><thead>{table.getHeaderGroups().map((group) => <tr key={group.id}>{group.headers.map((header) => <th key={header.id}>{flexRender(header.column.columnDef.header, header.getContext())}</th>)}</tr>)}</thead>
          <tbody>{table.getRowModel().rows.map((row) => <tr key={row.id}>{row.getVisibleCells().map((cell) => <td key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>)}</tr>)}</tbody></table></div>}
      {query.data && query.data.totalCount > 0 && <div className="table-footer"><label className="page-size-label">{t('common.perPage')}<select className="input select-native page-size" value={pageSize} onChange={(event) => setParam(setParams, 'pageSize', event.target.value)}><option value="12">12</option><option value="24">24</option><option value="48">48</option><option value="100">100</option></select></label>
        <span>{t('common.page')} {page} {t('common.of')} {Math.max(1, Math.ceil(query.data.totalCount / pageSize))} · {query.data.totalCount}</span>
        <div className="flex gap-2"><Button variant="secondary" size="sm" disabled={page <= 1} onClick={() => setParam(setParams, 'page', String(page - 1))}>{t('common.previous')}</Button><Button variant="secondary" size="sm" disabled={page * pageSize >= query.data.totalCount} onClick={() => setParam(setParams, 'page', String(page + 1))}>{t('common.next')}</Button></div>
      </div>}
    </section>
    <ConfirmDialog open={Boolean(deleteTarget)} onOpenChange={(open) => !open && setDeleteTarget(null)} title={t('product.deleteProduct')} description={t('product.deleteConfirm')} busy={remove.isPending} onConfirm={() => deleteTarget && remove.mutate(deleteTarget.id)} />
  </>;
}

export function ProductEditorPage() {
  const { t } = useTranslation();
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const editing = Boolean(id);
  const productQuery = useQuery({
    queryKey: ['product-edit', id],
    enabled: editing,
    queryFn: async () => {
      const responses = await Promise.all((['az', 'en', 'ru'] as const).map((language) => getProduct(id!, language)));
      const first = responses[0];
      const translations: Partial<Record<Language, ProductTranslation>> = {};
      for (const response of responses) translations[response.language as Language] = { name: response.name, description: response.description };
      return { ...first, translations };
    },
  });
  const [serverErrors, setServerErrors] = useState<Record<string, string[]> | undefined>();
  const mutation = useMutation({
    mutationFn: (request: ProductRequest) => editing ? updateProduct(id!, request) : createProduct(request),
    onSuccess: async (product) => {
      await queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success(t('product.productSaved'));
      navigate('/admin/products', { replace: true, state: { createdId: product.id } });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) setServerErrors(error.problem.errors);
      else toast.error(error instanceof Error ? error.message : t('errors.request'));
    },
  });
  if (productQuery.isPending && editing) return <LoadingState rows={5} />;
  if (productQuery.isError) return <ErrorState message={t('errors.notFound')} onRetry={() => void productQuery.refetch()} />;
  return <><PageHeader title={editing ? t('product.editTitle') : t('product.createTitle')} description={t('product.translationHint')} action={<Button variant="secondary" onClick={() => navigate('/admin/products')}>{t('common.cancel')}</Button>} />
    <section className="card form-card"><ProductForm product={productQuery.data} busy={mutation.isPending} onSave={(request) => mutation.mutate(request)} serverErrors={serverErrors} /></section></>;
}

function setParam(setParams: (next: URLSearchParams | ((current: URLSearchParams) => URLSearchParams), options?: { replace?: boolean }) => void, key: string, value: string) {
  setParams((current) => {
    const next = new URLSearchParams(current);
    if (value) next.set(key, value); else next.delete(key);
    if (key !== 'page') next.delete('page');
    return next;
  }, { replace: true });
}
