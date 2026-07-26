import { useEffect, useState } from 'react';
import { ApiError } from '../../../api/httpClient';
import { getCustomers } from './api';
import type { PagedResult } from '../../../api/types';
import type { CustomerListItem } from './types';

type CustomersState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: PagedResult<CustomerListItem> };

export function useAdminCustomers(page: number): CustomersState {
  const [state, setState] = useState<CustomersState>({ status: 'loading' });

  useEffect(() => {
    const controller = new AbortController();
    setState({ status: 'loading' });

    getCustomers({ page, pageSize: 20 }, controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, [page]);

  return state;
}
