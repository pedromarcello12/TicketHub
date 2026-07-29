import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { ingressosApi } from '../../api/ingressos'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'
import * as ui from '../../components/ui'

const STATUS_COLOR: Record<string, string> = {
  Disponivel: '#16a34a',
  Reservado: '#f59e0b',
  Vendido: '#3b82f6',
  Cancelado: '#dc2626',
}

export function IngressosAdminPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [eventoFiltro, setEventoFiltro] = useState('')

  const { data: eventos = [] } = useQuery({
    queryKey: ['admin-eventos'],
    queryFn: eventosApi.listar,
  })

  const { data: ingressos = [], isLoading } = useQuery({
    queryKey: ['admin-ingressos', eventoFiltro],
    queryFn: () => ingressosApi.listar(eventoFiltro || undefined),
  })

  const excluir = useMutation({
    mutationFn: ingressosApi.excluir,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-ingressos'] }),
  })

  const cancelar = useMutation({
    mutationFn: ingressosApi.cancelar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-ingressos'] }),
  })

  const eventoMap = Object.fromEntries(eventos.map((e) => [e.id, e.nome]))

  function confirmarExcluir(id: string) {
    if (confirm('Excluir este ingresso?')) excluir.mutate(id)
  }

  return (
    <Layout>
      <div style={ui.pageHeader}>
        <h1 style={ui.h1}>Ingressos</h1>
        <button style={ui.btnPrimary} onClick={() => navigate('/admin/ingressos/novo')}>
          + Novo ingresso
        </button>
      </div>

      <div style={{ marginBottom: 16 }}>
        <select
          value={eventoFiltro}
          onChange={(e) => setEventoFiltro(e.target.value)}
          style={{ ...ui.inputStyle, minWidth: 260 }}
        >
          <option value="">Todos os eventos</option>
          {eventos.map((ev) => (
            <option key={ev.id} value={ev.id}>{ev.nome}</option>
          ))}
        </select>
      </div>

      <div style={ui.card}>
        {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
        {!isLoading && ingressos.length === 0 && (
          <p style={{ color: '#888' }}>Nenhum ingresso encontrado.</p>
        )}

        {ingressos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table style={ui.table}>
              <thead>
                <tr>
                  <th style={ui.th}>Evento</th>
                  <th style={ui.th}>Tipo</th>
                  <th style={ui.th}>Preço</th>
                  <th style={ui.th}>Status</th>
                  <th style={ui.th}>Ações</th>
                </tr>
              </thead>
              <tbody>
                {ingressos.map((i) => (
                  <tr key={i.id}>
                    <td style={ui.td}>{eventoMap[i.eventoId] ?? i.eventoId.slice(0, 8)}</td>
                    <td style={ui.td}>{i.tipoIngresso}</td>
                    <td style={ui.td}>R$ {i.preco.toFixed(2).replace('.', ',')}</td>
                    <td style={ui.td}>
                      <span style={{
                        background: STATUS_COLOR[i.status] ?? '#6b7280',
                        color: '#fff',
                        padding: '2px 10px',
                        borderRadius: 20,
                        fontSize: 11,
                        fontWeight: 600,
                      }}>
                        {i.status}
                      </span>
                    </td>
                    <td style={{ ...ui.td, display: 'flex', gap: 6 }}>
                      {i.status === 'Disponivel' && (
                        <>
                          <button style={ui.btnSmall} onClick={() => navigate(`/admin/ingressos/${i.id}`)}>
                            Editar
                          </button>
                          <button style={ui.btnDanger} onClick={() => confirmarExcluir(i.id)}>
                            Excluir
                          </button>
                        </>
                      )}
                      {(i.status === 'Reservado' || i.status === 'Vendido') && (
                        <button style={{ ...ui.btnSmall, color: '#dc2626' }} onClick={() => cancelar.mutate(i.id)}>
                          Cancelar
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </Layout>
  )
}
