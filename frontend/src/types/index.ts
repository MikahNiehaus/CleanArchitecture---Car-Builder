export interface Car {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCarRequest {
  make: string;
  model: string;
  year: number;
  price: number;
  description?: string;
}

export interface UpdateCarRequest {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  description?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
}

export interface User {
  email: string;
  firstName: string | null;
  lastName: string | null;
}
