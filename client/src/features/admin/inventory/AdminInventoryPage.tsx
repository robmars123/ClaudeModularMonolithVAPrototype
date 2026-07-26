import { useState } from 'react';
import { useAdminInventory } from './useAdminInventory';
import { CreateInventoryItemForm } from './CreateInventoryItemForm';
import { AddStockControl } from './AddStockControl';
import { useProducts } from '../../products/useProducts';
import { Card } from '../../../components/ui/Card';
import { Button } from '../../../components/ui/Button';
import { Spinner, ErrorMessage, EmptyState, Badge } from '../../../components/ui/Feedback';

export function AdminInventoryPage() {
  const [page, setPage] = useState(1);
  const [lowStockOnly, setLowStockOnly] = useState(false);
  const { state, reload } = useAdminInventory(page, lowStockOnly);

  // Inventory rows only carry productId — look product names up from the catalog rather than
  // duplicating name/SKU fields on the inventory endpoint's response. GetProducts defaults to
  // active-only, so active and inactive are fetched separately to cover both (a product can be
  // deactivated without its inventory record going away).
  const activeProducts = useProducts({ page: 1, pageSize: 500, isActive: true });
  const inactiveProducts = useProducts({ page: 1, pageSize: 500, isActive: false });
  const productNames = new Map<string, string>([
    ...(activeProducts.status === 'ready' ? activeProducts.data.items.map((p): [string, string] => [p.id, p.name]) : []),
    ...(inactiveProducts.status === 'ready' ? inactiveProducts.data.items.map((p): [string, string] => [p.id, p.name]) : []),
  ]);

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Inventory</h1>

      <CreateInventoryItemForm onCreated={reload} />

      <label className="flex items-center gap-2 text-sm text-slate-700">
        <input
          type="checkbox"
          checked={lowStockOnly}
          onChange={(e) => {
            setLowStockOnly(e.target.checked);
            setPage(1);
          }}
        />
        Show low-stock items only
      </label>

      {state.status === 'loading' && <Spinner label="Loading inventory…" />}
      {state.status === 'error' && <ErrorMessage message={state.message} />}
      {state.status === 'ready' && state.data.items.length === 0 && (
        <EmptyState message="No inventory items found." />
      )}

      {state.status === 'ready' && state.data.items.length > 0 && (
        <>
          <Card className="overflow-x-auto p-0">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Product</th>
                  <th className="px-4 py-3">On hand</th>
                  <th className="px-4 py-3">Reserved</th>
                  <th className="px-4 py-3">Available</th>
                  <th className="px-4 py-3">Reorder level</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200">
                {state.data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium text-slate-900">
                      {productNames.get(item.productId) ?? item.productId}
                    </td>
                    <td className="px-4 py-3 text-slate-700">{item.quantityOnHand}</td>
                    <td className="px-4 py-3 text-slate-700">{item.reservedQuantity}</td>
                    <td className="px-4 py-3 text-slate-700">{item.availableQuantity}</td>
                    <td className="px-4 py-3 text-slate-700">{item.reorderLevel}</td>
                    <td className="px-4 py-3">
                      {item.needsReorder ? (
                        <Badge tone="warning">Needs reorder</Badge>
                      ) : (
                        <Badge tone="success">OK</Badge>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <AddStockControl productId={item.productId} onAdded={reload} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex items-center justify-between">
            <p className="text-sm text-slate-500">
              Page {state.data.page} of {state.data.totalPages || 1} · {state.data.totalCount} items
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
