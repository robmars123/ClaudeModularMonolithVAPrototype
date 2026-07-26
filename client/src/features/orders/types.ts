export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  status: string;
  totalAmount: number;
  shippingAddress: string | null;
  notes: string | null;
  items: OrderItem[];
  createdAt: string;
  updatedAt: string | null;
}

export interface PlaceOrderRequest {
  shippingAddress?: string;
  notes?: string;
}
