export type Role = 'Customer' | 'Admin' | 'ProductManager' | 'Support';

export interface AuthResponse {
  token: string;
  refreshToken: string;
  email: string;
  fullName: string;
  roles: string[];
  expiresAt: string;
}

export interface AuthUser {
  email: string;
  fullName: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface CreateStaffRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: Extract<Role, 'Admin' | 'ProductManager' | 'Support'>;
}

export interface StaffUserResponse {
  id: string;
  email: string;
  fullName: string;
  role: string;
}
