import api from './client'
import type { LoginRequest, LoginResponse, RegistrarRequest } from '../types'

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/api/auth/login', data).then((r) => r.data),

  registrar: (data: RegistrarRequest) =>
    api.post<LoginResponse>('/api/auth/registrar', data).then((r) => r.data),
}
