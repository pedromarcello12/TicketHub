import api from './client'
import type { CriarIngressoRequest, AtualizarIngressoRequest, IngressoResponse } from '../types'

export const ingressosApi = {
  listar: (eventoId?: string) =>
    api
      .get<IngressoResponse[]>('/api/ingressos', { params: eventoId ? { eventoId } : undefined })
      .then((r) => r.data),

  obterPorId: (id: string) =>
    api.get<IngressoResponse>(`/api/ingressos/${id}`).then((r) => r.data),

  criar: (data: CriarIngressoRequest) =>
    api.post<IngressoResponse>('/api/ingressos', data).then((r) => r.data),

  atualizar: (id: string, data: AtualizarIngressoRequest) =>
    api.put<IngressoResponse>(`/api/ingressos/${id}`, data).then((r) => r.data),

  excluir: (id: string) =>
    api.delete(`/api/ingressos/${id}`),

  reservar: (id: string) =>
    api.post<IngressoResponse>(`/api/ingressos/${id}/reservar`).then((r) => r.data),

  confirmarVenda: (id: string) =>
    api.post<IngressoResponse>(`/api/ingressos/${id}/confirmar-venda`).then((r) => r.data),

  cancelar: (id: string) =>
    api.post<IngressoResponse>(`/api/ingressos/${id}/cancelar`).then((r) => r.data),
}
