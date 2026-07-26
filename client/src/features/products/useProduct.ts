import { useEffect, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { getProduct } from './api';
import type { Product } from './types';

type ProductState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: Product };

export function useProduct(id: string | undefined): ProductState {
  const [state, setState] = useState<ProductState>({ status: 'loading' });

  useEffect(() => {
    if (!id) {
      setState({ status: 'error', message: 'No product specified.' });
      return;
    }

    const controller = new AbortController();
    setState({ status: 'loading' });

    getProduct(id, controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, [id]);

  return state;
}
