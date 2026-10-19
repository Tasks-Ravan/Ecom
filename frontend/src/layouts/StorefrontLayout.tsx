import { useQuery } from '@tanstack/react-query';
import { ArrowUpRight, Search, ShoppingBag, ShoppingCart, UserRound } from 'lucide-react';
import { useState, type FormEvent } from 'react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { listCategories } from '../api/categories';
import { LanguageSwitcher } from '../components/LanguageSwitcher';
import { ThemeToggle } from '../components/ThemeToggle';
import { Input } from '../components/ui/input';
import { useAuth } from '../lib/auth-context';
import { useTranslation } from 'react-i18next';
import { useCart } from '../lib/cart-context';

export function StorefrontLayout() {
  const { t } = useTranslation();
  const { user, isAdmin, signOut } = useAuth();
  const { itemCount } = useCart();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const categories = useQuery({ queryKey: ['categories'], queryFn: listCategories });
  const submitSearch = (event: FormEvent) => {
    event.preventDefault();
    const params = new URLSearchParams();
    if (search.trim()) params.set('search', search.trim());
    navigate(`/products${params.size ? `?${params.toString()}` : ''}`);
  };

  return <div className="store-shell">
    <header className="store-header">
      <div className="store-header-main page-container">
        <Link to="/" className="brand-lockup"><span className="brand-mark"><ShoppingBag size={18} /></span>
          <span><strong>{t('brand')}</strong><small>{t('tagline')}</small></span></Link>
        <form className="header-search" role="search" onSubmit={submitSearch}>
          <Search size={17} /><Input aria-label={t('common.search')} value={search} onChange={(event) => setSearch(event.target.value)} placeholder={t('product.searchPlaceholder')} />
          <button type="submit" className="sr-only">{t('common.search')}</button>
        </form>
        <div className="header-tools"><LanguageSwitcher /><ThemeToggle />
          <Link to="/cart" className="header-link cart-nav-link"><ShoppingCart size={17} /><span>{t('nav.cart', 'Cart')}</span>{itemCount > 0 && <span className="cart-count">{itemCount}</span>}</Link>
          {user ? <div className="account-menu"><span className="account-label"><UserRound size={17} />{user.email}</span>
            {isAdmin && <Link to="/admin" className="header-link">{t('nav.admin')}</Link>}
            <button className="header-link" onClick={() => { signOut(); navigate('/'); }}>{t('nav.logout')}</button>
          </div> : <Link to="/login" className="header-link"><UserRound size={16} />{t('nav.login')}</Link>}
        </div>
      </div>
      <div className="category-bar"><nav className="page-container category-nav" aria-label={t('nav.categories')}>
        <NavLink to="/products" className={({ isActive }) => `category-nav-link ${isActive ? 'is-current' : ''}`}>{t('nav.products')}</NavLink>
        {(categories.data ?? []).filter((category) => category.parentCategoryId === null).slice(0, 7).map((category) =>
          <NavLink key={category.id} to={`/products?categoryId=${category.id}`} className="category-nav-link">{category.name}</NavLink>)}
        <Link to="/products" className="category-nav-more">{t('nav.categories')} <ArrowUpRight size={13} /></Link>
      </nav></div>
    </header>
    <main className="store-main"><Outlet /></main>
    <Footer />
  </div>;
}

function Footer() {
  const { t } = useTranslation();
  const year = new Date().getFullYear();
  return <footer className="store-footer"><div className="page-container footer-content">
    <div className="brand-lockup"><span className="brand-mark"><ShoppingBag size={18} /></span>
      <span><strong>{t('brand')}</strong><small>{t('tagline')}</small></span></div>
    <p>© {year} {t('brand')}</p><Link to="/products" className="footer-link">{t('nav.products')} <ArrowUpRight size={14} /></Link>
  </div></footer>;
}

export function AuthLayout() {
  const { t } = useTranslation();
  return <div className="auth-shell"><header className="auth-top page-container">
    <Link to="/" className="brand-lockup"><span className="brand-mark"><ShoppingBag size={18} /></span><strong>{t('brand')}</strong></Link>
    <div className="header-tools"><LanguageSwitcher /><ThemeToggle /></div>
  </header><main className="auth-content"><Outlet /></main></div>;
}
