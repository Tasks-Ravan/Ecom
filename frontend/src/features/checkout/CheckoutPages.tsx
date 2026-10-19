import { useMutation, useQueries } from '@tanstack/react-query';
import { ArrowLeft, ArrowRight, CheckCircle2, CreditCard, Minus, PackageCheck, Plus, ShieldCheck, ShoppingBag, Trash2, XCircle } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'sonner';
import { checkout } from '../../api/checkout';
import { ApiError } from '../../api/client';
import { getProduct } from '../../api/products';
import { Button } from '../../components/ui/button';
import { EmptyState } from '../../components/States';
import { PageHeader } from '../../components/PageHeader';
import { useAuth } from '../../lib/auth-context';
import { useCart } from '../../lib/cart-context';
import { formatCurrency } from '../../lib/utils';
import type { CheckoutResponse, Language, ProductResponse } from '../../api/types';
import { useTranslation } from 'react-i18next';

const copy = {
  az: { cart: 'Səbət', cartBody: 'Sifarişinizi yoxlayın və davam edin.', empty: 'Səbətiniz boşdur.', unavailable: 'Məhsul artıq əlçatan deyil.', browse: 'Məhsullara bax', quantity: 'Miqdar', remove: 'Sil', subtotal: 'Cəmi', proceed: 'Ödənişə keç', checkout: 'Sifariş və ödəniş', checkoutBody: 'Məhsulları və yekun məbləği server yoxlayacaq.', signIn: 'Sifariş vermək üçün hesabınıza daxil olun.', signInButton: 'Daxil ol', sandbox: 'Test ödəniş rejimi', sandboxInfo: 'Bu sınaq rejimidir. Kart məlumatı istənilmir və real ödəniş aparılmır.', approve: 'Uğurlu ödənişi sına', decline: 'Rədd edilən ödənişi sına', pay: 'Test ödənişini et', processing: 'Emal edilir…', resultSuccess: 'Sifariş uğurla tamamlandı', resultFailed: 'Test ödənişi rədd edildi', order: 'Sifariş nömrəsi', charged: 'Yekun məbləğ', paid: 'Ödəniş alındı (sandbox)', failed: 'Ödəniş alınmadı, stok dəyişdirilmədi', back: 'Səbətə qayıt', stock: 'Stokda kifayət qədər məhsul yoxdur.' },
  en: { cart: 'Your cart', cartBody: 'Review your order before continuing.', empty: 'Your cart is empty.', unavailable: 'This product is no longer available.', browse: 'Browse products', quantity: 'Quantity', remove: 'Remove', subtotal: 'Subtotal', proceed: 'Continue to checkout', checkout: 'Checkout', checkoutBody: 'The server will verify current prices and stock before completing the order.', signIn: 'Sign in to place your order.', signInButton: 'Sign in', sandbox: 'Sandbox payment', sandboxInfo: 'This is a test flow. No card details are requested and no real payment is collected.', approve: 'Simulate approved payment', decline: 'Simulate declined payment', pay: 'Submit sandbox payment', processing: 'Processing…', resultSuccess: 'Order completed', resultFailed: 'Sandbox payment declined', order: 'Order number', charged: 'Final amount', paid: 'Payment approved (sandbox)', failed: 'No payment collected; stock was not changed', back: 'Return to cart', stock: 'There is not enough stock for this quantity.' },
  ru: { cart: 'Корзина', cartBody: 'Проверьте заказ перед продолжением.', empty: 'Корзина пуста.', unavailable: 'Этот товар больше недоступен.', browse: 'К товарам', quantity: 'Количество', remove: 'Удалить', subtotal: 'Итого', proceed: 'Перейти к оформлению', checkout: 'Оформление заказа', checkoutBody: 'Сервер проверит актуальные цены и остатки перед оформлением заказа.', signIn: 'Войдите, чтобы оформить заказ.', signInButton: 'Войти', sandbox: 'Тестовый платёж', sandboxInfo: 'Это тестовый сценарий. Данные карты не запрашиваются, реальная оплата не выполняется.', approve: 'Смоделировать успешную оплату', decline: 'Смоделировать отказ оплаты', pay: 'Выполнить тестовый платёж', processing: 'Обработка…', resultSuccess: 'Заказ оформлен', resultFailed: 'Тестовый платёж отклонён', order: 'Номер заказа', charged: 'Итоговая сумма', paid: 'Оплата одобрена (тест)', failed: 'Оплата не списана; остаток не изменён', back: 'Вернуться в корзину', stock: 'Недостаточно товара в наличии.' },
} as const;

function useCheckoutCopy() {
  const { i18n } = useTranslation();
  return copy[(i18n.language in copy ? i18n.language : 'en') as Language];
}

function useCartProducts() {
  const { items } = useCart();
  const { i18n } = useTranslation();
  const queries = useQueries({ queries: items.map((item) => ({
    queryKey: ['product', item.productId, i18n.language],
    queryFn: () => getProduct(item.productId, i18n.language),
  })) });
  return { items, queries };
}

export function CartPage() {
  const text = useCheckoutCopy();
  const { i18n } = useTranslation();
  const { items, queries } = useCartProducts();
  const { setQuantity, removeItem } = useCart();
  const lines = items.map((line, index) => ({ line, product: queries[index]?.data })).filter(
    (value): value is { line: typeof items[number]; product: ProductResponse } => Boolean(value.product));
  const loading = queries.some((query) => query.isPending);
  const total = lines.reduce((sum, { line, product }) => sum + product.price * line.quantity, 0);
  return <section className="page-container content-page">
    <PageHeader title={text.cart} description={text.cartBody} />
    {items.length === 0 ? <EmptyState title={text.empty} action={<Link to="/products" className="button button-primary button-default">{text.browse}</Link>} /> :
      <div className="cart-layout"><div className="card cart-lines">
        {lines.map(({ line, product }) => <article className="cart-line" key={line.productId}>
          <Link to={`/products/${product.id}`} className="cart-line-info"><span className="cart-line-mark"><ShoppingBag size={21} /></span>
            <span><strong>{product.name}</strong><small>{product.brandName} · {product.sku}</small><b>{formatCurrency(product.price, i18n.language)}</b></span></Link>
          <div className="cart-line-actions"><label><span>{text.quantity}</span><span className="quantity-stepper">
            <button type="button" aria-label={`${text.quantity} −`} disabled={line.quantity <= 1} onClick={() => setQuantity(line.productId, line.quantity - 1)}><Minus size={14} /></button>
            <span aria-label={text.quantity} aria-live="polite">{line.quantity}</span>
            <button type="button" aria-label={`${text.quantity} +`} disabled={line.quantity >= 20} onClick={() => setQuantity(line.productId, line.quantity + 1)}><Plus size={14} /></button>
          </span></label><strong>{formatCurrency(product.price * line.quantity, i18n.language)}</strong>
          <button className="icon-button cart-remove" type="button" aria-label={text.remove} onClick={() => removeItem(line.productId)}><Trash2 size={16} /></button></div>
        </article>)}
        {items.map((line, index) => queries[index]?.isError ? <article className="cart-line cart-unavailable" key={line.productId}><span>{text.unavailable}</span><button className="button button-ghost button-sm" type="button" onClick={() => removeItem(line.productId)}>{text.remove}</button></article> : null)}
        {loading && <p className="muted cart-loading">…</p>}
      </div><aside className="card cart-summary"><h2>{text.subtotal}</h2><div><span>{text.subtotal}</span><strong>{formatCurrency(total, i18n.language)}</strong></div>
        <p>{text.sandboxInfo}</p><Link className={`button button-primary button-default ${loading || lines.length !== items.length ? 'is-disabled' : ''}`} aria-disabled={loading || lines.length !== items.length} to={loading || lines.length !== items.length ? '#' : '/checkout'}>{text.proceed}<ArrowRight size={16} /></Link>
        <Link className="text-link" to="/products"><ArrowLeft size={15} />{text.browse}</Link></aside></div>}
  </section>;
}

export function CheckoutPage() {
  const text = useCheckoutCopy();
  const { i18n } = useTranslation();
  const { user } = useAuth();
  const { items, queries } = useCartProducts();
  const { clear } = useCart();
  const [simulateDecline, setSimulateDecline] = useState(false);
  const [result, setResult] = useState<CheckoutResponse | null>(null);
  const lines = items.map((line, index) => ({ line, product: queries[index]?.data })).filter(
    (value): value is { line: typeof items[number]; product: ProductResponse } => Boolean(value.product));
  const loading = queries.some((query) => query.isPending);
  const total = lines.reduce((sum, { line, product }) => sum + product.price * line.quantity, 0);
  const payment = useMutation({
    mutationFn: () => checkout({ items: items.map(({ productId, quantity }) => ({ productId, quantity })),
      simulateDecline, language: i18n.language as Language }),
    onSuccess: (response) => {
      setResult(response);
      if (response.status === 'Paid') clear();
    },
    onError: (error) => toast.error(error instanceof ApiError ? error.message : text.stock),
  });

  if (result) {
    const paid = result.status === 'Paid';
    return <section className="page-container content-page"><div className={`payment-result card ${paid ? 'payment-success' : 'payment-declined'}`}>
      <span className="payment-result-icon">{paid ? <CheckCircle2 size={32} /> : <XCircle size={32} />}</span>
      <h1>{paid ? text.resultSuccess : text.resultFailed}</h1><p>{paid ? text.paid : text.failed}</p>
      <dl><div><dt>{text.order}</dt><dd>{result.orderNumber}</dd></div><div><dt>{text.charged}</dt><dd>{formatCurrency(result.totalAmount, i18n.language)}</dd></div></dl>
      <Link className="button button-primary button-default" to={paid ? '/products' : '/cart'}>{paid ? text.browse : text.back}<ArrowRight size={16} /></Link>
    </div></section>;
  }

  if (!user) return <section className="page-container content-page"><PageHeader title={text.checkout} description={text.checkoutBody} />
    <div className="card checkout-login"><ShieldCheck size={25} /><p>{text.signIn}</p><Link to="/login" className="button button-primary button-default">{text.signInButton}<ArrowRight size={16} /></Link></div>
  </section>;

  if (!items.length) return <section className="page-container content-page"><EmptyState title={text.empty} action={<Link to="/products" className="button button-primary button-default">{text.browse}</Link>} /></section>;

  return <section className="page-container content-page"><PageHeader title={text.checkout} description={text.checkoutBody} />
    <div className="checkout-grid"><div className="card checkout-order"><h2><PackageCheck size={19} />{text.cart}</h2>
      {lines.map(({ line, product }) => <div className="checkout-line" key={line.productId}><span>{line.quantity} × {product.name}</span><strong>{formatCurrency(product.price * line.quantity, i18n.language)}</strong></div>)}
      <div className="checkout-total"><span>{text.subtotal}</span><strong>{formatCurrency(total, i18n.language)}</strong></div>
      <Link className="text-link" to="/cart"><ArrowLeft size={15} />{text.back}</Link></div>
      <form className="card sandbox-payment" onSubmit={(event) => { event.preventDefault(); payment.mutate(); }}>
        <span className="sandbox-badge"><CreditCard size={15} />{text.sandbox}</span><h2>{text.sandbox}</h2><p>{text.sandboxInfo}</p>
        <div className="sandbox-outcomes" role="radiogroup" aria-label={text.sandbox}>
          <button type="button" role="radio" aria-checked={!simulateDecline} className={!simulateDecline ? 'sandbox-outcome selected' : 'sandbox-outcome'} onClick={() => setSimulateDecline(false)}><CheckCircle2 size={18} /><span>{text.approve}</span></button>
          <button type="button" role="radio" aria-checked={simulateDecline} className={simulateDecline ? 'sandbox-outcome selected declined' : 'sandbox-outcome'} onClick={() => setSimulateDecline(true)}><XCircle size={18} /><span>{text.decline}</span></button>
        </div>
        {loading && <p className="muted">…</p>}
        <Button type="submit" disabled={loading || lines.length !== items.length || payment.isPending}>{payment.isPending ? text.processing : text.pay}<ShieldCheck size={16} /></Button>
      </form></div>
  </section>;
}
