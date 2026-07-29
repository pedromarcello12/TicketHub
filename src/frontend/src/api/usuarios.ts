import api from './client'
import type { UsuarioAdminResponse } from '../types'

export const usuariosApi = {
  listar: () =>
    api.get<UsuarioAdminResponse[]>('/api/usuarios').then((r) => r.data),

  excluir: (id: string) =>
    api.delete(`/api/usuarios/${id}`),
}
