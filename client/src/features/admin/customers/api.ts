import { getJson } from '../../../api/httpClient';
import type { PagedResult } from '../../../api/types';
import { buildQueryString } from '../../../lib/queryString';
import type { CustomerListItem } from './types';

export function getCustomers(
  params: { page?: number; pageSize?: number },
  signal?: AbortSignal,
): Promise<PagedResult<CustomerListItem>> {
  const query = buildQueryString(params);
  return getJson<PagedResult<CustomerListItem>>(`/api/customers${query}`, signal);
}
