import { api } from './api'

export interface AuthUser {
  id: number
  name: string
  email: string
}

export interface AuthResponse {
  user: AuthUser
  token: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  name: string
  email: string
  password: string
}


export async function login(credentials: LoginRequest): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/login', credentials)
  return response.data
}

export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/register', data)
  return response.data
}

export async function loginWithGoogle(idToken: string): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/google', { idToken })
  return response.data
}

export async function getCurrentUser(): Promise<AuthUser> {
  const response = await api.get<AuthUser>('/auth/me')
  return response.data
}