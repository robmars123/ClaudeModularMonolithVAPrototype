import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from './CartContext';
import { Card } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { Spinner, ErrorMessage, EmptyState } from '../../components/ui/Feedback';

export function CartPage() {
  const { cart, status, error, isMutating, updateItem, removeItem, clear } = useCart();
  const navigate = useNavigate();
  const [pendingProductId, setPendingProductId] = useState<string | null>(null);
  // Quantity edits are typed locally and only sent on blur/Enter — committing on every keystroke would
  // fire a PUT per digit, and since the server responses could land out of order, the displayed
  // quantity/total could end up disagreeing with what the server actually holds.
  const [draftQuantities, setDraftQuantities] = useState<Record<string, string>>({});

  const withPending = async (productId: string, action: () => Promise<void>) => {
    setPendingProductId(productId);
    try {
      await action();
    } finally {
      setPendingProductId(null);
    }
  };

  const commitQuantity = (productId: string, rawValue: string) => {
    setDraftQuantities((prev) => {
      const next = { ...prev };
      delete next[productId];
      return next;
    });

    const quantity = Number(rawValue);
    if (!Number.isFinite(quantity) || quantity < 1) return;

    const current = cart?.items.find((item) => item.productId === productId)?.quantity;
    if (quantity === current) return;

    void withPending(productId, () => updateItem(productId, quantity));
  };

  if (status === 'loading' || status === 'idle') return <Spinner label="Loading your cart…" />;
  if (status === 'error') return <ErrorMessage message={error ?? 'Could not load your cart.'} />;
  if (!cart || cart.items.length === 0) {
    return (
      <div className="flex flex-col gap-4">
        <h1 className="text-2xl font-semibold text-slate-900">Your cart</h1>
        <EmptyState message="Your cart is empty." />
        <Link to="/products" className="text-sm font-medium text-slate-900 underline">
          Browse products
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Your cart</h1>

      {error && <ErrorMessage message={error} />}

      <Card className="divide-y divide-slate-200 p-0">
        {cart.items.map((item) => {
          const rowPending = isMutating && pendingProductId === item.productId;
          return (
            <div key={item.id} className="flex items-center justify-between gap-4 px-6 py-4">
              <div>
                <p className="font-medium text-slate-900">{item.productName}</p>
                <p className="text-sm text-slate-500">${item.unitPrice.toFixed(2)} each</p>
              </div>
              <div className="flex items-center gap-3">
                <input
                  type="number"
                  min={1}
                  value={draftQuantities[item.productId] ?? item.quantity}
                  disabled={rowPending}
                  onChange={(e) =>
                    setDraftQuantities((prev) => ({ ...prev, [item.productId]: e.target.value }))
                  }
                  onBlur={(e) => commitQuantity(item.productId, e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') e.currentTarget.blur();
                  }}
                  className="w-16 rounded-md border border-slate-300 px-2 py-1 text-sm"
                />
                <p className="w-20 text-right text-sm font-semibold text-slate-900">${item.totalPrice.toFixed(2)}</p>
                <Button
                  variant="ghost"
                  size="sm"
                  pending={rowPending}
                  onClick={() => void withPending(item.productId, () => removeItem(item.productId))}
                >
                  Remove
                </Button>
              </div>
            </div>
          );
        })}
      </Card>

      <div className="flex items-center justify-between">
        <Button variant="secondary" pending={isMutating} onClick={() => void clear()}>
          Clear cart
        </Button>
        <div className="flex items-center gap-4">
          <p className="text-lg font-semibold text-slate-900">Total: ${cart.totalAmount.toFixed(2)}</p>
          <Button onClick={() => navigate('/checkout')}>Checkout</Button>
        </div>
      </div>
    </div>
  );
}
