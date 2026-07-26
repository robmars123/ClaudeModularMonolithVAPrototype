import { getJson, putJson } from '../../api/httpClient';
import type { Address, Customer, UpdateProfileRequest } from './types';

export function getMyProfile(signal?: AbortSignal): Promise<Customer> {
  return getJson<Customer>('/api/customers/me', signal);
}

export function updateMyProfile(request: UpdateProfileRequest, signal?: AbortSignal): Promise<Customer> {
  return putJson<Customer>('/api/customers/me', request, signal);
}

export function updateShippingAddress(request: Address, signal?: AbortSignal): Promise<void> {
  return putJson<void>('/api/customers/me/shipping-address', request, signal);
}
