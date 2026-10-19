import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';

export interface CartLine {
  productId: string;
  quantity: number;
}

interface CartContextValue {
  items: CartLine[];
  itemCount: number;
  addItem: (productId: string) => void;
  setQuantity: (productId: string, quantity: number) => void;
  removeItem: (productId: string) => void;
  clear: () => void;
}

const CartContext = createContext<CartContextValue | null>(null);
const STORAGE_KEY = 'ecommerce.cart';

function loadCart(): CartLine[] {
  try {
    const value: unknown = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '[]');
    if (!Array.isArray(value)) return [];
    return value.filter((item): item is CartLine => typeof item?.productId === 'string' &&
      Number.isInteger(item.quantity) && item.quantity > 0 && item.quantity <= 20);
  } catch {
    return [];
  }
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartLine[]>(loadCart);
  useEffect(() => { localStorage.setItem(STORAGE_KEY, JSON.stringify(items)); }, [items]);
  const update = useCallback((action: (current: CartLine[]) => CartLine[]) => {
    setItems(action);
  }, []);
  const addItem = useCallback((productId: string) => update((current) => {
    const existing = current.find((item) => item.productId === productId);
    if (existing) return current.map((item) => item.productId === productId
      ? { ...item, quantity: Math.min(20, item.quantity + 1) } : item);
    return [...current, { productId, quantity: 1 }];
  }), [update]);
  const setQuantity = useCallback((productId: string, quantity: number) => update((current) =>
    quantity <= 0 ? current.filter((item) => item.productId !== productId) : current.map((item) =>
      item.productId === productId ? { ...item, quantity: Math.min(20, Math.floor(quantity)) } : item)), [update]);
  const removeItem = useCallback((productId: string) => update((current) =>
    current.filter((item) => item.productId !== productId)), [update]);
  const clear = useCallback(() => update(() => []), [update]);
  const value = useMemo(() => ({ items, itemCount: items.reduce((count, item) => count + item.quantity, 0),
    addItem, setQuantity, removeItem, clear }), [items, addItem, setQuantity, removeItem, clear]);
  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) throw new Error('useCart must be used inside CartProvider.');
  return context;
}
