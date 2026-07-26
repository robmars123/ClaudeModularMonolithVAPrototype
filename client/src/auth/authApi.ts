import { getJson, postJson, postJsonPublic } from '../api/httpClient';
import type { AuthResponse, CreateStaffRequest, LoginRequest, RegisterRequest, StaffUserResponse } from './types';

export function login(request: LoginRequest, signal?: AbortSignal): Promise<AuthResponse> {
  return postJsonPublic<AuthResponse>('/api/auth/login', request, signal);
}

export function register(request: RegisterRequest, signal?: AbortSignal): Promise<AuthResponse> {
  return postJsonPublic<AuthResponse>('/api/auth/register', request, signal);
}

export function refresh(refreshToken: string, signal?: AbortSignal): Promise<AuthResponse> {
  return postJsonPublic<AuthResponse>('/api/auth/refresh', { refreshToken }, signal);
}

export function logout(refreshToken: string): Promise<void> {
  return postJsonPublic<void>('/api/auth/logout', { refreshToken });
}

export function createStaffAccount(request: CreateStaffRequest, signal?: AbortSignal): Promise<StaffUserResponse> {
  return postJson<StaffUserResponse>('/api/auth/staff', request, signal);
}

export interface UserProfileResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
  createdAt: string;
}

export function getCurrentUser(signal?: AbortSignal): Promise<UserProfileResponse> {
  return getJson<UserProfileResponse>('/api/auth/me', signal);
}
