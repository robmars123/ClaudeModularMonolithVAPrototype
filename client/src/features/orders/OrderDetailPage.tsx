import { Link, useParams } from 'react-router-dom';
import { useOrder } from './useOrder';
import { orderStatusTone } from './orderStatus';
import { Card } from '../../components/ui/Card';
import { Spinner, ErrorMessage, Badge } from '../../components/ui/Feedback';

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const state = useOrder(id);

  if (state.status === 'loading') return <Spinner label="Loading order…" />;
  if (state.status === 'error') return <ErrorMessage message={state.message} />;

  const order = state.data;

  return (
    <div className="flex flex-col gap-4">
      <Link to="/orders" className="text-sm text-slate-500 hover:text-slate-800">
        ← Back to orders
      </Link>

      <Card className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-xl font-semibold text-slate-900">{order.orderNumber}</h1>
            <p className="text-xs text-slate-500">{new Date(order.createdAt).toLocaleString()}</p>
          </div>
          <Badge tone={orderStatusTone(order.status)}>{order.status}</Badge>
        </div>

        {order.shippingAddress && (
          <div>
            <p className="text-xs font-medium uppercase text-slate-400">Shipping address</p>
            <p className="text-sm text-slate-700">{order.shippingAddress}</p>
          </div>
        )}

        {order.notes && (
          <div>
            <p className="text-xs font-medium uppercase text-slate-400">Notes</p>
            <p className="text-sm text-slate-700">{order.notes}</p>
          </div>
        )}

        <div className="divide-y divide-slate-200 rounded-md border border-slate-200">
          {order.items.map((item) => (
            <div key={item.id} className="flex items-center justify-between px-4 py-3">
              <div>
                <p className="text-sm font-medium text-slate-900">{item.productName}</p>
                <p className="text-xs text-slate-500">
                  {item.quantity} × ${item.unitPrice.toFixed(2)}
                </p>
              </div>
              <p className="text-sm font-semibold text-slate-900">${item.totalPrice.toFixed(2)}</p>
            </div>
          ))}
        </div>

        <div className="flex justify-end">
          <p className="text-lg font-semibold text-slate-900">Total: ${order.totalAmount.toFixed(2)}</p>
        </div>
      </Card>
    </div>
  );
}
