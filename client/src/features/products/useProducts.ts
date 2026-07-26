import { useEffect, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { getProducts } from './api';
import type { PagedResult } from '../../api/types';
import type { Product, ProductListParams } from './types';

type ProductsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: PagedResult<Product> };

export function useProducts(params: ProductListParams): ProductsState {
  const [state, setState] = useState<ProductsState>({ status: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    setState({ status: 'loading' });

    getProducts(params, controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, [params.page, params.pageSize, params.search, params.categoryId, params.isActive]);

  return state;
}
