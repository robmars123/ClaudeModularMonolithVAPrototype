import { useCallback, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { placeOrder } from '../orders/api';
import type { Order, PlaceOrderRequest } from '../orders/types';

type CheckoutState =
  | { status: 'idle' }
  | { status: 'submitting' }
  | { status: 'success'; order: Order }
  | { status: 'error'; message: string };

export function useCheckout() {
  const [state, setState] = useState<CheckoutState>({ status: 'idle' });

  const submit = useCallback(async (request: PlaceOrderRequest) => {
    setState({ status: 'submitting' });
    try {
      const order = await placeOrder(request);
      setState({ status: 'success', order });
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
      setState({ status: 'error', message });
    }
  }, []);

  return { state, submit };
}
