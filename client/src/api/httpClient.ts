const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5163';

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

export async function postJson<TResponse>(
  path: string,
  body: unknown,
  signal?: AbortSignal,
): Promise<TResponse> {
  const response = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
    signal,
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    const message = data?.errors?.join(', ') ?? data?.error ?? 'Request failed.';
    throw new ApiError(message, response.status);
  }

  return data as TResponse;
}
