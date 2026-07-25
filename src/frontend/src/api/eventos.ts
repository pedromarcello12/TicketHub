import api from './client'
import type { CriarEventoRequest, EventoResponse } from '../types'

export const eventosApi = {
  listar: () =>
    api.get<EventoResponse[]>('/api/eventos').then((r) => r.data),

  obterPorId: (id: string) =>
    api.get<EventoResponse>(`/api/eventos/${id}`).then((r) => r.data),

  criar: (data: CriarEventoRequest) =>
    api.post<EventoResponse>('/api/eventos', data).then((r) => r.data),

  publicar: (id: string) =>
    api.post<EventoResponse>(`/api/eventos/${id}/publicar`).then((r) => r.data),

  cancelar: (id: string) =>
    api.post<EventoResponse>(`/api/eventos/${id}/cancelar`).then((r) => r.data),

  encerrar: (id: string) =>
    api.post<EventoResponse>(`/api/eventos/${id}/encerrar`).then((r) => r.data),
}
