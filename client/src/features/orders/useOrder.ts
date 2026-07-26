import { useEffect, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { getOrder } from './api';
import type { Order } from './types';

type OrderState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: Order };

export function useOrder(orderId: string | undefined): OrderState {
  const [state, setState] = useState<OrderState>({ status: 'loading' });

  useEffect(() => {
    if (!orderId) {
      setState({ status: 'error', message: 'No order specified.' });
      return;
    }

    const controller = new AbortController();
    setState({ status: 'loading' });

    getOrder(orderId, controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, [orderId]);

  return state;
}
