import { deleteJson, getJson, postJson, putJson } from '../../api/httpClient';
import type { PagedResult } from '../../api/types';
import { buildQueryString } from '../../lib/queryString';
import type {
  Category,
  CreateCategoryRequest,
  CreateProductRequest,
  Product,
  ProductListParams,
  UpdateProductRequest,
} from './types';

export function getProducts(params: ProductListParams = {}, signal?: AbortSignal): Promise<PagedResult<Product>> {
  const query = buildQueryString(params);
  return getJson<PagedResult<Product>>(`/api/products${query}`, signal);
}

export function getProduct(id: string, signal?: AbortSignal): Promise<Product> {
  return getJson<Product>(`/api/products/${id}`, signal);
}

export function getCategories(signal?: AbortSignal): Promise<Category[]> {
  return getJson<Category[]>('/api/categories', signal);
}

export function createProduct(request: CreateProductRequest, signal?: AbortSignal): Promise<Product> {
  return postJson<Product>('/api/products', request, signal);
}

export function updateProduct(id: string, request: UpdateProductRequest, signal?: AbortSignal): Promise<Product> {
  return putJson<Product>(`/api/products/${id}`, request, signal);
}

export function deleteProduct(id: string, signal?: AbortSignal): Promise<void> {
  return deleteJson<void>(`/api/products/${id}`, signal);
}

export function createCategory(request: CreateCategoryRequest, signal?: AbortSignal): Promise<Category> {
  return postJson<Category>('/api/categories', request, signal);
}
