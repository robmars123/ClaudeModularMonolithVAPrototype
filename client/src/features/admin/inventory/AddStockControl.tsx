import { useState } from 'react';
import { addStock } from './api';
import { ApiError } from '../../../api/httpClient';
import { Button } from '../../../components/ui/Button';

export function AddStockControl({ productId, onAdded }: { productId: string; onAdded: () => void }) {
  const [quantity, setQuantity] = useState('10');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAdd = async () => {
    const value = Number(quantity);
    if (!Number.isFinite(value) || value <= 0) {
      setError('Enter a positive quantity.');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await addStock(productId, { productId, quantity: value });
      onAdded();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col items-end gap-1">
      <div className="flex items-center gap-2">
        <input
          type="number"
          min={1}
          value={quantity}
          disabled={submitting}
          onChange={(e) => setQuantity(e.target.value)}
          className="w-20 rounded-md border border-slate-300 px-2 py-1 text-sm"
        />
        <Button size="sm" variant="secondary" pending={submitting} onClick={() => void handleAdd()}>
          Add stock
        </Button>
      </div>
      {error && <span className="text-xs text-red-600">{error}</span>}
    </div>
  );
}
