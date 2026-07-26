import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useCheckout } from './useCheckout';
import { useCart } from '../cart/CartContext';
import { Card } from '../../components/ui/Card';
import { Textarea } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { Spinner, ErrorMessage, EmptyState } from '../../components/ui/Feedback';

export function CheckoutPage() {
  const { cart, status: cartStatus, error: cartError, refresh } = useCart();
  const { state, submit } = useCheckout();
  const [shippingAddress, setShippingAddress] = useState('');
  const [notes, setNotes] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    void submit({
      shippingAddress: shippingAddress.trim() || undefined,
      notes: notes.trim() || undefined,
    }).then(() => void refresh());
  };

  if (state.status === 'success') {
    const order = state.order;
    return (
      <Card className="mx-auto max-w-lg text-center">
        <h1 className="text-xl font-semibold text-slate-900">Order placed</h1>
        <p className="mt-2 text-sm text-slate-500">
          Your order <span className="font-medium text-slate-700">{order.orderNumber}</span> has been placed for $
          {order.totalAmount.toFixed(2)}.
        </p>
        <Link to={`/orders/${order.id}`} className="mt-6 inline-block text-sm font-medium text-slate-900 underline">
          View order details
        </Link>
      </Card>
    );
  }

  if (cartStatus === 'loading' || cartStatus === 'idle') return <Spinner label="Loading your cart…" />;

  if (cartStatus === 'error') {
    return (
      <div className="flex flex-col gap-4">
        <h1 className="text-2xl font-semibold text-slate-900">Checkout</h1>
        <ErrorMessage message={cartError ?? 'Could not load your cart.'} />
        <Button variant="secondary" onClick={() => void refresh()} className="w-fit">
          Try again
        </Button>
      </div>
    );
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="flex flex-col gap-4">
        <h1 className="text-2xl font-semibold text-slate-900">Checkout</h1>
        <EmptyState message="Your cart is empty — add products before checking out." />
        <Link to="/products" className="text-sm font-medium text-slate-900 underline">
          Browse products
        </Link>
      </div>
    );
  }

  const isSubmitting = state.status === 'submitting';

  return (
    <div className="mx-auto flex max-w-lg flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Checkout</h1>

      <Card>
        <h2 className="text-sm font-medium text-slate-700">Order summary</h2>
        <div className="mt-3 divide-y divide-slate-200">
          {cart.items.map((item) => (
            <div key={item.id} className="flex items-center justify-between py-2 text-sm">
              <span className="text-slate-700">
                {item.productName} × {item.quantity}
              </span>
              <span className="font-medium text-slate-900">${item.totalPrice.toFixed(2)}</span>
            </div>
          ))}
        </div>
        <div className="mt-3 flex justify-between border-t border-slate-200 pt-3 text-sm font-semibold text-slate-900">
          <span>Total</span>
          <span>${cart.totalAmount.toFixed(2)}</span>
        </div>
      </Card>

      <Card>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <Textarea
            id="shippingAddress"
            label="Shipping address (optional)"
            hint="Leave blank to use your saved profile address."
            rows={3}
            value={shippingAddress}
            onChange={(e) => setShippingAddress(e.target.value)}
          />
          <Textarea
            id="notes"
            label="Order notes (optional)"
            rows={2}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />

          {state.status === 'error' && <ErrorMessage message={state.message} />}

          <Button type="submit" pending={isSubmitting} className="w-full">
            {isSubmitting ? 'Placing order…' : 'Place order'}
          </Button>
        </form>
      </Card>
    </div>
  );
}
