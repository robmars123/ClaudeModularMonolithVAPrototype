import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useProducts } from './useProducts';
import { useCategories } from './useCategories';
import type { ProductListParams } from './types';
import { Card } from '../../components/ui/Card';
import { Input, Select } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { Spinner, ErrorMessage, EmptyState } from '../../components/ui/Feedback';
import { AddToCartButton } from '../cart/AddToCartButton';

const PAGE_SIZE = 12;

export function ProductListPage() {
  const [searchInput, setSearchInput] = useState('');
  const [params, setParams] = useState<ProductListParams>({ page: 1, pageSize: PAGE_SIZE, isActive: true });

  const productsState = useProducts(params);
  const categoriesState = useCategories();

  const handleSearchSubmit = (e: FormEvent) => {
    e.preventDefault();
    setParams((prev) => ({ ...prev, search: searchInput || undefined, page: 1 }));
  };

  const handleCategoryChange = (categoryId: string) => {
    setParams((prev) => ({ ...prev, categoryId: categoryId || undefined, page: 1 }));
  };

  const goToPage = (page: number) => setParams((prev) => ({ ...prev, page }));

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Products</h1>
        <p className="mt-1 text-sm text-slate-500">Browse the catalog.</p>
      </div>

      <Card className="flex flex-wrap items-end gap-4">
        <form onSubmit={handleSearchSubmit} className="flex items-end gap-2">
          <Input
            id="search"
            label="Search"
            placeholder="Search products…"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          <Button type="submit" variant="secondary">
            Search
          </Button>
        </form>

        <Select
          id="category"
          label="Category"
          value={params.categoryId ?? ''}
          onChange={(e) => handleCategoryChange(e.target.value)}
        >
          <option value="">All categories</option>
          {categoriesState.status === 'ready' &&
            categoriesState.data.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
        </Select>
      </Card>

      {productsState.status === 'loading' && <Spinner label="Loading products…" />}
      {productsState.status === 'error' && <ErrorMessage message={productsState.message} />}
      {productsState.status === 'ready' && productsState.data.items.length === 0 && (
        <EmptyState message="No products match your filters." />
      )}

      {productsState.status === 'ready' && productsState.data.items.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {productsState.data.items.map((product) => (
              <Card key={product.id} className="flex flex-col justify-between">
                <div>
                  {product.imageUrl && (
                    <img
                      src={product.imageUrl}
                      alt={product.name}
                      className="mb-3 h-32 w-full rounded-md object-cover"
                    />
                  )}
                  <Link to={`/products/${product.id}`} className="font-medium text-slate-900 hover:underline">
                    {product.name}
                  </Link>
                  <p className="mt-1 text-xs text-slate-500">{product.categoryName}</p>
                  <p className="mt-2 text-sm font-semibold text-slate-900">${product.price.toFixed(2)}</p>
                </div>
                <div className="mt-4">
                  <AddToCartButton productId={product.id} />
                </div>
              </Card>
            ))}
          </div>

          <div className="flex items-center justify-between">
            <p className="text-sm text-slate-500">
              Page {productsState.data.page} of {productsState.data.totalPages || 1} · {productsState.data.totalCount}{' '}
              products
            </p>
            <div className="flex gap-2">
              <Button
                variant="secondary"
                size="sm"
                disabled={!productsState.data.hasPreviousPage}
                onClick={() => goToPage(productsState.data.page - 1)}
              >
                Previous
              </Button>
              <Button
                variant="secondary"
                size="sm"
                disabled={!productsState.data.hasNextPage}
                onClick={() => goToPage(productsState.data.page + 1)}
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
