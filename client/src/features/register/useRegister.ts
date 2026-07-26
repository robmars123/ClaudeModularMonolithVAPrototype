import { useCallback, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { register } from '../../auth/authApi';
import { useAuth } from '../../auth/AuthContext';
import type { RegisterRequest } from '../../auth/types';

type RegisterState = { status: 'idle' } | { status: 'submitting' } | { status: 'error'; message: string };

export function useRegister() {
  const { applySession } = useAuth();
  const [state, setState] = useState<RegisterState>({ status: 'idle' });

  const submit = useCallback(
    async (request: RegisterRequest) => {
      setState({ status: 'submitting' });
      try {
        const response = await register(request);
        applySession(response);
      } catch (err) {
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      }
    },
    [applySession],
  );

  return { state, submit };
}
