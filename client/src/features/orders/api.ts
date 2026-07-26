import { getJson, postJson } from '../../api/httpClient';
import type { Order, PlaceOrderRequest } from './types';

export function placeOrder(request: PlaceOrderRequest, signal?: AbortSignal): Promise<Order> {
  return postJson<Order>('/api/orders/place', request, signal);
}

export function getOrders(signal?: AbortSignal): Promise<Order[]> {
  return getJson<Order[]>('/api/orders', signal);
}

export function getOrder(orderId: string, signal?: AbortSignal): Promise<Order> {
  return getJson<Order>(`/api/orders/${orderId}`, signal);
}
