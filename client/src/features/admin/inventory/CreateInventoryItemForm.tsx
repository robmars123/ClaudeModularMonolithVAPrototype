import { useState, type FormEvent } from 'react';
import { createInventoryItem } from './api';
import { useProducts } from '../../products/useProducts';
import { ApiError } from '../../../api/httpClient';
import { Card } from '../../../components/ui/Card';
import { Input, Select } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';
import { ErrorMessage } from '../../../components/ui/Feedback';

export function CreateInventoryItemForm({ onCreated }: { onCreated: () => void }) {
  const productsState = useProducts({ page: 1, pageSize: 100, isActive: true });
  const [productId, setProductId] = useState('');
  const [initialQuantity, setInitialQuantity] = useState('0');
  const [reorderLevel, setReorderLevel] = useState('0');
  const [reorderQuantity, setReorderQuantity] = useState('0');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const products = productsState.status === 'ready' ? productsState.data.items : [];

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!productId) {
      setError('Select a product.');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await createInventoryItem({
        productId,
        initialQuantity: Number(initialQuantity),
        reorderLevel: Number(reorderLevel),
        reorderQuantity: Number(reorderQuantity),
      });
      onCreated();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Card>
      <h2 className="text-sm font-medium text-slate-700">Track a new product</h2>
      <form onSubmit={(e) => void handleSubmit(e)} className="mt-4 flex flex-wrap items-end gap-3">
        <Select id="productId" label="Product" required value={productId} onChange={(e) => setProductId(e.target.value)}>
          <option value="" disabled>
            Select a product
          </option>
          {products.map((product) => (
            <option key={product.id} value={product.id}>
              {product.name}
            </option>
          ))}
        </Select>
        <Input
          id="initialQuantity"
          label="Initial quantity"
          type="number"
          min={0}
          value={initialQuantity}
          onChange={(e) => setInitialQuantity(e.target.value)}
        />
        <Input
          id="reorderLevel"
          label="Reorder level"
          type="number"
          min={0}
          value={reorderLevel}
          onChange={(e) => setReorderLevel(e.target.value)}
        />
        <Input
          id="reorderQuantity"
          label="Reorder quantity"
          type="number"
          min={0}
          value={reorderQuantity}
          onChange={(e) => setReorderQuantity(e.target.value)}
        />
        <Button type="submit" pending={submitting}>
          Add
        </Button>
      </form>
      {error && <div className="mt-3"><ErrorMessage message={error} /></div>}
    </Card>
  );
}
