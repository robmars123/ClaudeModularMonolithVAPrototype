import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import * as cartApi from './api';
import type { Cart } from './types';
import { ApiError } from '../../api/httpClient';
import { useAuth } from '../../auth/AuthContext';

interface CartContextValue {
  cart: Cart | null;
  status: 'idle' | 'loading' | 'ready' | 'error';
  error: string | null;
  itemCount: number;
  isMutating: boolean;
  refresh: () => Promise<void>;
  addItem: (productId: string, quantity: number) => Promise<void>;
  updateItem: (productId: string, quantity: number) => Promise<void>;
  removeItem: (productId: string) => Promise<void>;
  clear: () => Promise<void>;
}

const CartContext = createContext<CartContextValue | undefined>(undefined);

function errorMessage(err: unknown): string {
  return err instanceof ApiError ? err.message : 'Could not reach the server.';
}

export function CartProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated, status: authStatus } = useAuth();
  const [cart, setCart] = useState<Cart | null>(null);
  const [status, setStatus] = useState<'idle' | 'loading' | 'ready' | 'error'>('idle');
  const [error, setError] = useState<string | null>(null);
  const [isMutating, setIsMutating] = useState(false);

  const refresh = useCallback(async () => {
    setStatus('loading');
    setError(null);
    try {
      const data = await cartApi.getCart();
      setCart(data);
      setStatus('ready');
    } catch (err) {
      setError(errorMessage(err));
      setStatus('error');
    }
  }, []);

  useEffect(() => {
    if (authStatus !== 'ready') return;
    if (isAuthenticated) {
      void refresh();
    } else {
      setCart(null);
      setStatus('idle');
      setError(null);
    }
  }, [authStatus, isAuthenticated, refresh]);

  const runMutation = useCallback(async (mutation: () => Promise<Cart | void>) => {
    setIsMutating(true);
    setError(null);
    try {
      const result = await mutation();
      if (result) {
        setCart(result);
      } else {
        await refresh();
      }
    } catch (err) {
      setError(errorMessage(err));
      throw err;
    } finally {
      setIsMutating(false);
    }
  }, [refresh]);

  const addItem = useCallback(
    (productId: string, quantity: number) => runMutation(() => cartApi.addCartItem(productId, quantity)),
    [runMutation],
  );

  const updateItem = useCallback(
    (productId: string, quantity: number) => runMutation(() => cartApi.updateCartItem(productId, quantity)),
    [runMutation],
  );

  const removeItem = useCallback(
    (productId: string) => runMutation(() => cartApi.removeCartItem(productId)),
    [runMutation],
  );

  const clear = useCallback(() => runMutation(() => cartApi.clearCart()), [runMutation]);

  const itemCount = useMemo(
    () => cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
    [cart],
  );

  const value: CartContextValue = {
    cart,
    status,
    error,
    itemCount,
    isMutating,
    refresh,
    addItem,
    updateItem,
    removeItem,
    clear,
  };

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart(): CartContextValue {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used within a CartProvider');
  return ctx;
}
