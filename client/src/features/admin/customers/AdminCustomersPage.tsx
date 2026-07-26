import { useState } from 'react';
import { useAdminCustomers } from './useAdminCustomers';
import { Card } from '../../../components/ui/Card';
import { Button } from '../../../components/ui/Button';
import { Spinner, ErrorMessage, EmptyState } from '../../../components/ui/Feedback';

export function AdminCustomersPage() {
  const [page, setPage] = useState(1);
  const state = useAdminCustomers(page);

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Customers</h1>

      {state.status === 'loading' && <Spinner label="Loading customers…" />}
      {state.status === 'error' && <ErrorMessage message={state.message} />}
      {state.status === 'ready' && state.data.items.length === 0 && (
        <EmptyState message="No customers found." />
      )}

      {state.status === 'ready' && state.data.items.length > 0 && (
        <>
          <Card className="overflow-x-auto p-0">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">Email</th>
                  <th className="px-4 py-3">Phone</th>
                  <th className="px-4 py-3">Joined</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {state.data.items.map((customer) => (
                  <tr key={customer.id}>
                    <td className="px-4 py-3 font-medium text-slate-900">{customer.fullName}</td>
                    <td className="px-4 py-3 text-slate-500">{customer.email}</td>
                    <td className="px-4 py-3 text-slate-500">{customer.phone ?? '—'}</td>
                    <td className="px-4 py-3 text-slate-500">
                      {new Date(customer.createdAt).toLocaleDateString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex items-center justify-between">
            <p className="text-sm text-slate-500">
              Page {state.data.page} of {state.data.totalPages || 1} · {state.data.totalCount} customers
            </p>
            <div className="flex gap-2">
              <Button
                variant="secondary"
                size="sm"
                disabled={!state.data.hasPreviousPage}
                onClick={() => setPage((p) => p - 1)}
              >
                Previous
              </Button>
              <Button
                variant="secondary"
                size="sm"
                disabled={!state.data.hasNextPage}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
