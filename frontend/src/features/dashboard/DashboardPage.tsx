import { useQuery } from '@tanstack/react-query';
import { Activity, ArrowRight, Boxes, CircleDollarSign, Package, PackageX } from 'lucide-react';
import { Link } from 'react-router-dom';
import { listBrands } from '../../api/brands';
import { listCategories } from '../../api/categories';
import { listProducts } from '../../api/products';
import type { ProductResponse } from '../../api/types';
import { ErrorState, LoadingState } from '../../components/States';
import { PageHeader } from '../../components/PageHeader';
import { Badge } from '../../components/ui/badge';
import { formatCurrency, formatDate } from '../../lib/utils';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../lib/auth-context';

async function loadAllProducts() {
  const first = await listProducts({ page: 1, pageSize: 100, sort: 'created_desc' });
  const pages = await Promise.all(Array.from({ length: Math.ceil(first.totalCount / 100) - 1 }, (_, index) =>
    listProducts({ page: index + 2, pageSize: 100, sort: 'created_desc' })));
  return [first, ...pages].flatMap((page) => page.items);
}

export function DashboardPage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const products = useQuery({ queryKey: ['dashboard-products'], queryFn: loadAllProducts });
  const categories = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  const brands = useQuery({ queryKey: ['brands'], queryFn: listBrands });
  const error = products.isError || categories.isError || brands.isError;
  const retry = () => { void products.refetch(); void categories.refetch(); void brands.refetch(); };
  if (products.isPending || categories.isPending || brands.isPending) return <><PageHeader title={t('admin.dashboard')} description={t('admin.overview')} /><LoadingState rows={5} /></>;
  if (error) return <ErrorState onRetry={retry} />;
  const allProducts = products.data ?? [];
  const lowStock = [...allProducts].filter((product) => product.stockQuantity <= 5).sort((a, b) => a.stockQuantity - b.stockQuantity).slice(0, 6);
  const latest = [...allProducts].sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt)).slice(0, 5);
  const stats = [
    { label: t('admin.totalProducts'), value: allProducts.length, icon: Package, tone: 'lavender' },
    { label: t('admin.activeProducts'), value: allProducts.filter((product) => product.isActive).length, icon: Activity, tone: 'mint' },
    { label: t('admin.outOfStock'), value: allProducts.filter((product) => product.stockQuantity === 0).length, icon: PackageX, tone: 'peach' },
    { label: t('admin.totalCategories'), value: categories.data.length, icon: Boxes, tone: 'blue' },
    { label: t('admin.totalBrands'), value: brands.data.length, icon: CircleDollarSign, tone: 'yellow' },
  ];
  return <>
    <PageHeader title={t('admin.dashboard')} description={`${t('admin.welcome')}, ${user?.email ?? ''}. ${t('admin.overview')}`} action={<Link to="/admin/products/new" className="button button-primary button-default">{t('product.createProduct')}<ArrowRight size={16} /></Link>} />
    <div className="stat-grid">{stats.map((stat) => { const Icon = stat.icon; return <article className="stat-card" key={stat.label}>
      <span className={`stat-icon ${stat.tone}`}><Icon size={19} /></span><span className="stat-label">{stat.label}</span><strong className="stat-value">{stat.value.toLocaleString(i18n.language)}</strong><span className="stat-foot">{t('admin.catalogSnapshot')}</span>
    </article>; })}</div>
    <div className="dashboard-columns">
      <section className="card dashboard-list-card"><div className="card-heading"><div><h2>{t('admin.lowStock')}</h2><p>{t('admin.lowStockHint')}</p></div><Badge>{lowStock.length}</Badge></div>
        {lowStock.length === 0 ? <div className="table-empty">{t('admin.stockHealthy')}</div> : <div className="table-scroll"><table className="data-table"><thead><tr><th>{t('common.name')}</th><th>{t('product.sku')}</th><th>{t('common.stock')}</th></tr></thead><tbody>{lowStock.map((product) => <DashboardProductRow key={product.id} product={product} />)}</tbody></table></div>}
      </section>
      <section className="card dashboard-list-card"><div className="card-heading"><div><h2>{t('admin.recentProducts')}</h2><p>{t('admin.recentHint')}</p></div><Link to="/admin/products" className="text-link">{t('admin.allProducts')}<ArrowRight size={14} /></Link></div>
        <div className="recent-list">{latest.map((product) => <Link to={`/admin/products/${product.id}/edit`} className="recent-item" key={product.id}><span className="table-product-mark">P</span><span className="recent-info"><strong>{product.name}</strong><small>{product.categoryName} · {formatDate(product.createdAt, i18n.language)}</small></span><strong>{formatCurrency(product.price, i18n.language)}</strong></Link>)}</div>
      </section>
    </div>
    <div className="quick-links"><Link to="/admin/categories" className="quick-link-card"><span className="quick-icon"><Boxes size={18} /></span><span><strong>{t('admin.manageCategories')}</strong><small>{categories.data.length.toLocaleString(i18n.language)} {t('admin.totalCategories').toLocaleLowerCase(i18n.language)}</small></span><ArrowRight size={16} /></Link>
      <Link to="/admin/brands" className="quick-link-card"><span className="quick-icon"><CircleDollarSign size={18} /></span><span><strong>{t('admin.manageBrands')}</strong><small>{brands.data.length.toLocaleString(i18n.language)} {t('admin.totalBrands').toLocaleLowerCase(i18n.language)}</small></span><ArrowRight size={16} /></Link></div>
  </>;
}

function DashboardProductRow({ product }: { product: ProductResponse }) {
  const { t } = useTranslation();
  return <tr><td><Link className="table-product-name" to={`/admin/products/${product.id}/edit`}>{product.name}<small>{product.sku}</small></Link></td><td>{product.sku}</td><td><Badge className="status-warning">{product.stockQuantity} {t('product.stockLow').toLocaleLowerCase()}</Badge></td></tr>;
}
