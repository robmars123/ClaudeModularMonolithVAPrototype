import { useCallback, useState } from 'react';
import { ApiError } from '../../api/httpClient';
import { useAuth } from '../../auth/AuthContext';

type LoginState = { status: 'idle' } | { status: 'submitting' } | { status: 'error'; message: string };

export function useLogin() {
  const { login } = useAuth();
  const [state, setState] = useState<LoginState>({ status: 'idle' });

  const submit = useCallback(
    async (email: string, password: string) => {
      setState({ status: 'submitting' });
      try {
        await login(email, password);
      } catch (err) {
        const message = err instanceof ApiError ? err.message : 'Could not reach the server.';
        setState({ status: 'error', message });
      }
    },
    [login],
  );

  return { state, submit };
}
