import { useCallback, useEffect, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { getMyProfile } from './api';
import type { Customer } from './types';

type ProfileState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: Customer };

export function useProfile() {
  const [state, setState] = useState<ProfileState>({ status: 'loading' });

  const load = useCallback(() => {
    const controller = new AbortController();
    setState({ status: 'loading' });

    getMyProfile(controller.signal)
      .then((data) => setState({ status: 'ready', data }))
      .catch((err: unknown) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      });

    return controller;
  }, []);

  useEffect(() => {
    const controller = load();
    return () => controller.abort();
  }, [load]);

  return { state, reload: load, setState };
}
