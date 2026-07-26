import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../../../api/httpClient';
import { getProducts } from '../../products/api';
import type { PagedResult } from '../../../api/types';
import type { Product } from '../../products/types';

type AdminProductsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: PagedResult<Product> };

export function useAdminProducts(page: number, isActive: boolean) {
  const [state, setState] = useState<AdminProductsState>({ status: 'loading' });

  const load = useCallback(() => {
    const controller = new AbortController();
    setState({ status: 'loading' });

    getProducts({ page, pageSize: 20, isActive }, controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return controller;
  }, [page, isActive]);

  useEffect(() => {
    const controller = load();
    return () => controller.abort();
  }, [load]);

  return { state, reload: load };
}
