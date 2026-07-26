import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../../../api/httpClient';
import { getInventory } from './api';
import type { PagedResult } from '../../../api/types';
import type { InventoryItem } from './types';

type InventoryState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: PagedResult<InventoryItem> };

export function useAdminInventory(page: number, lowStockOnly: boolean) {
  const [state, setState] = useState<InventoryState>({ status: 'loading' });

  const load = useCallback(() => {
    const controller = new AbortController();
    setState({ status: 'loading' });

    getInventory({ page, pageSize: 20, lowStockOnly }, controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return controller;
  }, [page, lowStockOnly]);

  useEffect(() => {
    const controller = load();
    return () => controller.abort();
  }, [load]);

  return { state, reload: load };
}
