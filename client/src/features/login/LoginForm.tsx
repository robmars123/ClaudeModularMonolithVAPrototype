import { useEffect, useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useLogin } from './useLogin';
import { useAuth } from '../../auth/AuthContext';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { ErrorMessage } from '../../components/ui/Feedback';

export function LoginForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { state, submit } = useLogin();
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as { from?: { pathname: string } } | null)?.from;

  useEffect(() => {
    if (isAuthenticated) navigate(from?.pathname ?? '/products', { replace: true });
  }, [isAuthenticated, navigate, from]);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    void submit(email, password);
  };

  const isSubmitting = state.status === 'submitting';

  return (
    <Card className="w-full max-w-sm">
      <h1 className="text-xl font-semibold text-slate-900">Log in</h1>

      <form onSubmit={handleSubmit} className="mt-6 flex flex-col gap-4">
        <Input
          id="email"
          label="Email"
          type="email"
          autoComplete="username"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <Input
          id="password"
          label="Password"
          type="password"
          autoComplete="current-password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        {state.status === 'error' && <ErrorMessage message={state.message} />}

        <Button type="submit" pending={isSubmitting} className="w-full">
          {isSubmitting ? 'Logging in…' : 'Log in'}
        </Button>
      </form>

      <p className="mt-4 text-center text-sm text-slate-500">
        Don't have an account?{' '}
        <Link to="/register" className="font-medium text-slate-900 underline">
          Register
        </Link>
      </p>
    </Card>
  );
}
