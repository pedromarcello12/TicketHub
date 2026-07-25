import api from './client'
import type { CriarPagamentoRequest, PagamentoResponse } from '../types'

export const pagamentoApi = {
  listar: (ingressoId?: string) =>
    api
      .get<PagamentoResponse[]>('/api/pagamentos', {
        params: ingressoId ? { ingressoId } : undefined,
      })
      .then((r) => r.data),

  obterPorId: (id: string) =>
    api.get<PagamentoResponse>(`/api/pagamentos/${id}`).then((r) => r.data),

  criar: (data: CriarPagamentoRequest) =>
    api.post<PagamentoResponse>('/api/pagamentos', data).then((r) => r.data),

  aprovar: (id: string) =>
    api.post<PagamentoResponse>(`/api/pagamentos/${id}/aprovar`).then((r) => r.data),

  recusar: (id: string) =>
    api.post<PagamentoResponse>(`/api/pagamentos/${id}/recusar`).then((r) => r.data),

  estornar: (id: string) =>
    api.post<PagamentoResponse>(`/api/pagamentos/${id}/estornar`).then((r) => r.data),
}
