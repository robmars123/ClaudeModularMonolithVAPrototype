import { useState } from 'react';
import { useAdminProducts } from './useAdminProducts';
import { ProductForm } from './ProductForm';
import { CategoryQuickCreate } from './CategoryQuickCreate';
import { useCategories } from '../../products/useCategories';
import { deleteProduct } from '../../products/api';
import type { Product } from '../../products/types';
import { ApiError } from '../../../api/httpClient';
import { Card } from '../../../components/ui/Card';
import { Select } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';
import { Spinner, ErrorMessage, EmptyState, Badge } from '../../../components/ui/Feedback';
import { useAuth } from '../../../auth/AuthContext';

export function AdminProductsPage() {
  const { user } = useAuth();
  const canDelete = user?.roles.includes('Admin') ?? false;
  const [page, setPage] = useState(1);
  const [isActive, setIsActive] = useState(true);
  const [editing, setEditing] = useState<Product | null | 'new'>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { state, reload } = useAdminProducts(page, isActive);
  const categoriesState = useCategories();
  const categories = categoriesState.status === 'ready' ? categoriesState.data : [];

  const handleDelete = async (product: Product) => {
    if (!confirm(`Delete "${product.name}"? This cannot be undone.`)) return;
    setDeletingId(product.id);
    setDeleteError(null);
    try {
      await deleteProduct(product.id);
      reload();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Manage products</h1>
        {editing === null && <Button onClick={() => setEditing('new')}>New product</Button>}
      </div>

      <Card>
        <CategoryQuickCreate
          onCreated={() => {
            reload();
            categoriesState.reload();
          }}
        />
      </Card>

      {editing !== null && (
        <ProductForm
          key={editing === 'new' ? 'new' : editing.id}
          product={editing === 'new' ? null : editing}
          categories={categories}
          onSaved={() => {
            setEditing(null);
            reload();
          }}
          onCancel={() => setEditing(null)}
        />
      )}

      <Select
        id="isActiveFilter"
        label="Status"
        value={isActive ? 'active' : 'inactive'}
        onChange={(e) => {
          setIsActive(e.target.value === 'active');
          setPage(1);
        }}
        className="max-w-xs"
      >
        <option value="active">Active</option>
        <option value="inactive">Inactive</option>
      </Select>

      {state.status === 'loading' && <Spinner label="Loading products…" />}
      {state.status === 'error' && <ErrorMessage message={state.message} />}
      {deleteError && <ErrorMessage message={deleteError} />}

      {state.status === 'ready' && state.data.items.length === 0 && (
        <EmptyState message="No products found for this filter." />
      )}

      {state.status === 'ready' && state.data.items.length > 0 && (
        <>
          <Card className="overflow-x-auto p-0">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">SKU</th>
                  <th className="px-4 py-3">Category</th>
                  <th className="px-4 py-3">Price</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {state.data.items.map((product) => (
                  <tr key={product.id}>
                    <td className="px-4 py-3 font-medium text-slate-900">{product.name}</td>
                    <td className="px-4 py-3 text-slate-500">{product.sku}</td>
                    <td className="px-4 py-3 text-slate-500">{product.categoryName}</td>
                    <td className="px-4 py-3 text-slate-700">${product.price.toFixed(2)}</td>
                    <td className="px-4 py-3">
                      <Badge tone={product.isActive ? 'success' : 'neutral'}>
                        {product.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <Button variant="secondary" size="sm" onClick={() => setEditing(product)}>
                          Edit
                        </Button>
                        {canDelete && (
                          <Button
                            variant="danger"
                            size="sm"
                            pending={deletingId === product.id}
                            onClick={() => void handleDelete(product)}
                          >
                            Delete
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex items-center justify-between">
            <p className="text-sm text-slate-500">
              Page {state.data.page} of {state.data.totalPages || 1} · {state.data.totalCount} products
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
