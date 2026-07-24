import { useState, type FormEvent } from 'react';
import { useRegister } from './useRegister';

export function RegisterForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { state, submit } = useRegister();

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    submit({ email, password });
  };

  if (state.status === 'success') {
    return (
      <div className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-8 text-center shadow-sm">
        <h1 className="text-xl font-semibold text-slate-900">Account created</h1>
        <p className="mt-2 text-sm text-slate-500">
          Welcome, {state.data.email}. You're registered as {state.data.roles.join(', ')}.
        </p>
      </div>
    );
  }

  const isSubmitting = state.status === 'submitting';

  return (
    <form
      onSubmit={handleSubmit}
      className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-8 shadow-sm"
    >
      <h1 className="text-xl font-semibold text-slate-900">Create an account</h1>

      <label className="mt-6 block text-sm font-medium text-slate-700" htmlFor="email">
        Username
      </label>
      <input
        id="email"
        type="email"
        autoComplete="username"
        required
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-slate-500 focus:ring-1 focus:ring-slate-500"
      />

      <label className="mt-4 block text-sm font-medium text-slate-700" htmlFor="password">
        Password
      </label>
      <input
        id="password"
        type="password"
        autoComplete="new-password"
        required
        minLength={6}
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-slate-500 focus:ring-1 focus:ring-slate-500"
      />

      {state.status === 'error' && (
        <p className="mt-4 text-sm text-red-600">{state.message}</p>
      )}

      <button
        type="submit"
        disabled={isSubmitting}
        className="mt-6 w-full rounded-md bg-slate-900 py-2 text-sm font-medium text-white transition hover:bg-slate-700 disabled:opacity-50"
      >
        {isSubmitting ? 'Creating account…' : 'Register'}
      </button>
    </form>
  );
}
