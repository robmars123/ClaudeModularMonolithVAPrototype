import { postJson } from '../../api/httpClient';
import type { AuthResponse, RegisterRequest } from './types';

export interface RegisterCredentials {
  email: string;
  password: string;
}

// The API's account record has no bare "username" field — email doubles as
// the login identifier, and first/last name are required by the user store.
// Since this form is intentionally email + password only, we fill a
// placeholder name here rather than asking the user for it.
function toRegisterRequest({ email, password }: RegisterCredentials): RegisterRequest {
  return { email, password, firstName: 'New', lastName: 'User' };
}

export function register(credentials: RegisterCredentials, signal?: AbortSignal) {
  return postJson<AuthResponse>('/api/auth/register', toRegisterRequest(credentials), signal);
}
