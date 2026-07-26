import { useState, type FormEvent } from 'react';
import { createProduct, updateProduct } from '../../products/api';
import type { Category, Product } from '../../products/types';
import { ApiError } from '../../../api/httpClient';
import { Card } from '../../../components/ui/Card';
import { Input, Select, Textarea } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';
import { ErrorMessage } from '../../../components/ui/Feedback';

interface ProductFormProps {
  product: Product | null;
  categories: Category[];
  onSaved: () => void;
  onCancel: () => void;
}

export function ProductForm({ product, categories, onSaved, onCancel }: ProductFormProps) {
  const [name, setName] = useState(product?.name ?? '');
  const [description, setDescription] = useState(product?.description ?? '');
  const [sku, setSku] = useState(product?.sku ?? '');
  const [price, setPrice] = useState(product ? String(product.price) : '');
  const [categoryId, setCategoryId] = useState(product?.categoryId ?? categories[0]?.id ?? '');
  const [imageUrl, setImageUrl] = useState(product?.imageUrl ?? '');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = product !== null;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const priceValue = Number(price);
    if (!Number.isFinite(priceValue) || priceValue < 0) {
      setError('Enter a valid price.');
      return;
    }
    if (!categoryId) {
      setError('Select a category.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      if (isEditing) {
        await updateProduct(product.id, {
          id: product.id,
          name,
          description,
          price: priceValue,
          categoryId,
          imageUrl: imageUrl || undefined,
        });
      } else {
        await createProduct({
          name,
          description,
          sku,
          price: priceValue,
          categoryId,
          imageUrl: imageUrl || undefined,
        });
      }
      onSaved();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Card>
      <h2 className="text-sm font-medium text-slate-700">{isEditing ? 'Edit product' : 'New product'}</h2>
      <form onSubmit={(e) => void handleSubmit(e)} className="mt-4 flex flex-col gap-4">
        <Input id="name" label="Name" required value={name} onChange={(e) => setName(e.target.value)} />
        <Textarea
          id="description"
          label="Description"
          rows={3}
          required
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <div className="grid grid-cols-2 gap-3">
          <Input
            id="sku"
            label="SKU"
            required
            disabled={isEditing}
            hint={isEditing ? 'SKU cannot be changed.' : undefined}
            value={sku}
            onChange={(e) => setSku(e.target.value)}
          />
          <Input
            id="price"
            label="Price"
            type="number"
            min={0}
            step="0.01"
            required
            value={price}
            onChange={(e) => setPrice(e.target.value)}
          />
        </div>
        <Select
          id="categoryId"
          label="Category"
          required
          value={categoryId}
          onChange={(e) => setCategoryId(e.target.value)}
        >
          <option value="" disabled>
            Select a category
          </option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </Select>
        <Input
          id="imageUrl"
          label="Image URL (optional)"
          value={imageUrl}
          onChange={(e) => setImageUrl(e.target.value)}
        />

        {error && <ErrorMessage message={error} />}

        <div className="flex gap-3">
          <Button type="submit" pending={submitting}>
            {isEditing ? 'Save changes' : 'Create product'}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel} disabled={submitting}>
            Cancel
          </Button>
        </div>
      </form>
    </Card>
  );
}
