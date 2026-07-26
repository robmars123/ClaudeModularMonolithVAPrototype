import { useEffect, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { getOrders } from './api';
import type { Order } from './types';

type OrdersState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: Order[] };

export function useOrders(): OrdersState {
  const [state, setState] = useState<OrdersState>({ status: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    setState({ status: 'loading' });

    getOrders(controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, []);

  return state;
}
