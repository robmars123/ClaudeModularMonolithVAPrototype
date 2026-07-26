export interface InventoryItem {
  id: string;
  productId: string;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderLevel: number;
  reorderQuantity: number;
  needsReorder: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateInventoryItemRequest {
  productId: string;
  initialQuantity: number;
  reorderLevel: number;
  reorderQuantity: number;
}

export interface AddStockRequest {
  productId: string;
  quantity: number;
}
