import { useCallback, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { register, type RegisterCredentials } from './authApi';
import type { AuthResponse } from './types';

type RegisterState =
  | { status: 'idle' }
  | { status: 'submitting' }
  | { status: 'success'; data: AuthResponse }
  | { status: 'error'; message: string };

export function useRegister() {
  const [state, setState] = useState<RegisterState>({ status: 'idle' });

  const submit = useCallback(async (credentials: RegisterCredentials) => {
    setState({ status: 'submitting' });
    try {
      const data = await register(credentials);
      setState({ status: 'success', data });
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
      setState({ status: 'error', message });
    }
  }, []);

  return { state, submit };
}
