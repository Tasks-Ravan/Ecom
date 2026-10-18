import { BarChart3, Boxes, BriefcaseBusiness, ChevronLeft, ChevronRight, FileSpreadsheet, Layers3, LogOut, Menu, Package, X } from 'lucide-react';
import { useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { LanguageSwitcher } from '../components/LanguageSwitcher';
import { ThemeToggle } from '../components/ThemeToggle';
import { useAuth } from '../lib/auth-context';
import { useTranslation } from 'react-i18next';

const navItems = [
  { to: '/admin', key: 'dashboard', icon: BarChart3, end: true },
  { to: '/admin/products', key: 'products', icon: Package },
  { to: '/admin/categories', key: 'categories', icon: Layers3 },
  { to: '/admin/brands', key: 'brands', icon: Boxes },
  { to: '/admin/exports', key: 'exports', icon: FileSpreadsheet },
];

export function AdminLayout() {
  const { t } = useTranslation();
  const { user, signOut } = useAuth();
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const active = navItems.find((item) => item.end ? location.pathname === item.to : location.pathname.startsWith(item.to));
  const side = <>
    <div className="admin-brand"><Link to="/admin" onClick={() => setMobileOpen(false)} className="brand-lockup">
      <span className="brand-mark"><BriefcaseBusiness size={18} /></span><span><strong>{t('brand')}</strong><small>{t('admin.dashboardTitle')}</small></span>
    </Link><button className="icon-button sidebar-close" aria-label={t('common.close')} onClick={() => setMobileOpen(false)}><X size={18} /></button></div>
    <nav className="admin-nav" aria-label={t('admin.navigation')}>
      <span className="sidebar-label">{t('admin.workspace')}</span>
      {navItems.map((item) => { const Icon = item.icon; return <NavLink key={item.to} to={item.to} end={item.end}
        onClick={() => setMobileOpen(false)} className={({ isActive }) => `admin-nav-link ${isActive ? 'active' : ''}`}>
        <Icon size={18} /><span>{t(`admin.${item.key}`)}</span>{location.pathname.startsWith(item.to) && !item.end ? <ChevronRight size={15} className="nav-arrow" /> : null}
      </NavLink>; })}
      <a className="admin-nav-link" href="/hangfire" target="_blank" rel="noreferrer"><BriefcaseBusiness size={18} /><span>{t('admin.hangfire')}</span><ArrowExternal /></a>
    </nav>
    <div className="sidebar-bottom"><Link to="/" className="storefront-link" onClick={() => setMobileOpen(false)}><ChevronLeft size={16} />{t('admin.backToStore')}</Link>
      <button className="admin-user" onClick={() => { signOut(); navigate('/login'); }}><span className="avatar">{user?.email.slice(0, 1).toUpperCase()}</span>
        <span className="user-meta"><strong>{user?.email}</strong><small>{t('admin.administrator')}</small></span><LogOut size={16} /></button>
    </div>
  </>;
  return <div className="admin-shell">
    <aside className="admin-sidebar desktop-sidebar">{side}</aside>
    {mobileOpen && <div className="mobile-sidebar-backdrop" onClick={() => setMobileOpen(false)}>
      <aside className="admin-sidebar mobile-sidebar" onClick={(event) => event.stopPropagation()}>{side}</aside>
    </div>}
    <div className="admin-main-wrap"><header className="admin-topbar">
      <button className="utility-button mobile-menu-button" onClick={() => setMobileOpen(true)} aria-label={t('nav.openMenu')}><Menu size={19} /></button>
      <div className="breadcrumbs"><span>{t('admin.admin')}</span><ChevronRight size={14} /><strong>{t(`admin.${active?.key ?? 'dashboard'}`)}</strong></div>
      <div className="header-tools"><LanguageSwitcher /><ThemeToggle /><span className="topbar-user">{user?.email}</span></div>
    </header><main className="admin-content"><Outlet /></main>
    </div>
  </div>;
}

function ArrowExternal() { return <span className="external-indicator" aria-hidden="true">↗</span>; }
