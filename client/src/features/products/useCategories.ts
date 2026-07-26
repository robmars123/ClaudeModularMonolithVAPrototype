import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { getCategories } from './api';
import type { Category } from './types';

type CategoriesState =
  | { status: 'loading'; reload: () => void }
  | { status: 'error'; message: string; reload: () => void }
  | { status: 'ready'; data: Category[]; reload: () => void };

export function useCategories(): CategoriesState {
  const [data, setData] = useState<Category[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [version, setVersion] = useState(0);

  const reload = useCallback(() => setVersion((v) => v + 1), []);

  useEffect(() => {
    const controller = new AbortController();

    getCategories(controller.signal)
      .then((result) => {
        setData(result);
        setError(null);
      })
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(err instanceof ApiError ? err.message : 'Could not reach the server.');
      });

    return () => controller.abort();
  }, [version]);

  if (error) return { status: 'error', message: error, reload };
  if (data) return { status: 'ready', data, reload };
  return { status: 'loading', reload };
}
