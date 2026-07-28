import api from './client'
import type { LoginRequest, LoginResponse, RegistrarRequest, PerfilResponse, AtualizarPerfilRequest } from '../types'

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/api/auth/login', data).then((r) => r.data),

  registrar: (data: RegistrarRequest) =>
    api.post<LoginResponse>('/api/auth/registrar', data).then((r) => r.data),

  obterPerfil: () =>
    api.get<PerfilResponse>('/api/auth/me').then((r) => r.data),

  atualizarPerfil: (data: AtualizarPerfilRequest) =>
    api.patch<PerfilResponse>('/api/auth/me', data).then((r) => r.data),
}
