import { getJson, postJson } from '../../../api/httpClient';
import type { PagedResult } from '../../../api/types';
import { buildQueryString } from '../../../lib/queryString';
import type { AddStockRequest, CreateInventoryItemRequest, InventoryItem } from './types';

export function getInventory(
  params: { page?: number; pageSize?: number; lowStockOnly?: boolean },
  signal?: AbortSignal,
): Promise<PagedResult<InventoryItem>> {
  const query = buildQueryString(params);
  return getJson<PagedResult<InventoryItem>>(`/api/inventory${query}`, signal);
}

export function createInventoryItem(request: CreateInventoryItemRequest, signal?: AbortSignal): Promise<InventoryItem> {
  return postJson<InventoryItem>('/api/inventory', request, signal);
}

export function addStock(productId: string, request: AddStockRequest, signal?: AbortSignal): Promise<InventoryItem> {
  return postJson<InventoryItem>(`/api/inventory/${productId}/add-stock`, request, signal);
}
