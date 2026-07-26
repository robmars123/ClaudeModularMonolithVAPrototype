import { deleteJson, getJson, postJson, putJson } from '../../api/httpClient';
import type { Cart } from './types';

export function getCart(signal?: AbortSignal): Promise<Cart> {
  return getJson<Cart>('/api/cart', signal);
}

export function addCartItem(productId: string, quantity: number, signal?: AbortSignal): Promise<Cart> {
  return postJson<Cart>('/api/cart/items', { productId, quantity }, signal);
}

export function updateCartItem(productId: string, quantity: number, signal?: AbortSignal): Promise<Cart> {
  return putJson<Cart>(`/api/cart/items/${productId}`, { productId, quantity }, signal);
}

export function removeCartItem(productId: string, signal?: AbortSignal): Promise<void> {
  return deleteJson<void>(`/api/cart/items/${productId}`, signal);
}

export function clearCart(signal?: AbortSignal): Promise<void> {
  return deleteJson<void>('/api/cart', signal);
}
