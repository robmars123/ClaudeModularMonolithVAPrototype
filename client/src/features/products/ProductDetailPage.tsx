import { Link, useParams } from 'react-router-dom';
import { useProduct } from './useProduct';
import { Card } from '../../components/ui/Card';
import { Spinner, ErrorMessage, Badge } from '../../components/ui/Feedback';
import { AddToCartButton } from '../cart/AddToCartButton';

export function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const state = useProduct(id);

  if (state.status === 'loading') return <Spinner label="Loading product…" />;
  if (state.status === 'error') return <ErrorMessage message={state.message} />;

  const product = state.data;

  return (
    <div className="flex flex-col gap-4">
      <Link to="/products" className="text-sm text-slate-500 hover:text-slate-800">
        ← Back to products
      </Link>

      <Card className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {product.imageUrl && (
          <img src={product.imageUrl} alt={product.name} className="h-64 w-full rounded-md object-cover" />
        )}
        <div className="flex flex-col gap-3">
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-semibold text-slate-900">{product.name}</h1>
            {!product.isActive && <Badge tone="warning">Inactive</Badge>}
          </div>
          <p className="text-sm text-slate-500">{product.categoryName}</p>
          <p className="text-sm text-slate-700">{product.description}</p>
          <p className="text-xs text-slate-400">SKU: {product.sku}</p>
          <p className="text-xl font-semibold text-slate-900">${product.price.toFixed(2)}</p>
          <div>
            <AddToCartButton productId={product.id} disabled={!product.isActive} />
          </div>
        </div>
      </Card>
    </div>
  );
}
