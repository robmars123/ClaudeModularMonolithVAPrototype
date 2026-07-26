import { Link } from 'react-router-dom';
import { useOrders } from './useOrders';
import { orderStatusTone } from './orderStatus';
import { Card } from '../../components/ui/Card';
import { Spinner, ErrorMessage, EmptyState, Badge } from '../../components/ui/Feedback';

export function OrdersListPage() {
  const state = useOrders();

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Your orders</h1>

      {state.status === 'loading' && <Spinner label="Loading orders…" />}
      {state.status === 'error' && <ErrorMessage message={state.message} />}
      {state.status === 'ready' && state.data.length === 0 && (
        <EmptyState message="You haven't placed any orders yet." />
      )}

      {state.status === 'ready' && state.data.length > 0 && (
        <Card className="divide-y divide-slate-200 p-0">
          {state.data.map((order) => (
            <Link
              key={order.id}
              to={`/orders/${order.id}`}
              className="flex items-center justify-between gap-4 px-6 py-4 hover:bg-slate-50"
            >
              <div>
                <p className="font-medium text-slate-900">{order.orderNumber}</p>
                <p className="text-xs text-slate-500">{new Date(order.createdAt).toLocaleString()}</p>
              </div>
              <div className="flex items-center gap-4">
                <Badge tone={orderStatusTone(order.status)}>{order.status}</Badge>
                <p className="w-20 text-right text-sm font-semibold text-slate-900">
                  ${order.totalAmount.toFixed(2)}
                </p>
              </div>
            </Link>
          ))}
        </Card>
      )}
    </div>
  );
}
