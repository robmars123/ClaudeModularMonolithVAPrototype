import { Link } from 'react-router-dom';

export function ForbiddenPage() {
  return (
    <div className="mx-auto max-w-md py-20 text-center">
      <h1 className="text-2xl font-semibold text-slate-900">403 — Access denied</h1>
      <p className="mt-2 text-sm text-slate-500">
        You don't have permission to view this page.
      </p>
      <Link to="/" className="mt-6 inline-block text-sm font-medium text-slate-900 underline">
        Back to home
      </Link>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <div className="mx-auto max-w-md py-20 text-center">
      <h1 className="text-2xl font-semibold text-slate-900">404 — Page not found</h1>
      <p className="mt-2 text-sm text-slate-500">The page you're looking for doesn't exist.</p>
      <Link to="/" className="mt-6 inline-block text-sm font-medium text-slate-900 underline">
        Back to home
      </Link>
    </div>
  );
}
