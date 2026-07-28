import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'
import * as ui from '../../components/ui'

const STATUS_COLOR: Record<string, string> = {
  Planejado: '#6b7280',
  Publicado: '#16a34a',
  Cancelado: '#dc2626',
  Encerrado: '#92400e',
}

export function EventosAdminPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { data: eventos = [], isLoading } = useQuery({
    queryKey: ['admin-eventos'],
    queryFn: eventosApi.listar,
  })

  const publicar = useMutation({
    mutationFn: eventosApi.publicar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }),
  })

  const cancelar = useMutation({
    mutationFn: eventosApi.cancelar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }),
  })

  const encerrar = useMutation({
    mutationFn: eventosApi.encerrar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }),
  })

  const excluir = useMutation({
    mutationFn: eventosApi.excluir,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }),
  })

  function confirmarExcluir(id: string, nome: string) {
    if (confirm(`Excluir evento "${nome}"?`)) excluir.mutate(id)
  }

  return (
    <Layout>
      <div style={ui.pageHeader}>
        <h1 style={ui.h1}>Eventos</h1>
        <button style={ui.btnPrimary} onClick={() => navigate('/admin/eventos/novo')}>
          + Novo evento
        </button>
      </div>

      <div style={ui.card}>
        {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
        {!isLoading && eventos.length === 0 && (
          <p style={{ color: '#888' }}>Nenhum evento cadastrado.</p>
        )}

        {eventos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table style={ui.table}>
              <thead>
                <tr>
                  <th style={ui.th}>Nome</th>
                  <th style={ui.th}>Local</th>
                  <th style={ui.th}>Data / Hora</th>
                  <th style={ui.th}>Capacidade</th>
                  <th style={ui.th}>Status</th>
                  <th style={ui.th}>Ações</th>
                </tr>
              </thead>
              <tbody>
                {eventos.map((ev) => (
                  <tr key={ev.id}>
                    <td style={ui.td}>{ev.nome}</td>
                    <td style={ui.td}>{ev.local}</td>
                    <td style={ui.td}>{new Date(ev.dataHora).toLocaleString('pt-BR')}</td>
                    <td style={ui.td}>{ev.capacidadeTotal}</td>
                    <td style={ui.td}>
                      <span style={{
                        background: STATUS_COLOR[ev.status] ?? '#6b7280',
                        color: '#fff',
                        padding: '2px 10px',
                        borderRadius: 20,
                        fontSize: 11,
                        fontWeight: 600,
                      }}>
                        {ev.status}
                      </span>
                    </td>
                    <td style={{ ...ui.td, display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                      {ev.status === 'Planejado' && (
                        <>
                          <button style={ui.btnSmall} onClick={() => navigate(`/admin/eventos/${ev.id}`)}>
                            Editar
                          </button>
                          <button style={ui.btnSmall} onClick={() => publicar.mutate(ev.id)}>
                            Publicar
                          </button>
                          <button style={ui.btnDanger} onClick={() => confirmarExcluir(ev.id, ev.nome)}>
                            Excluir
                          </button>
                        </>
                      )}
                      {ev.status === 'Publicado' && (
                        <>
                          <button style={ui.btnSmall} onClick={() => encerrar.mutate(ev.id)}>
                            Encerrar
                          </button>
                          <button style={{ ...ui.btnSmall, color: '#dc2626' }} onClick={() => cancelar.mutate(ev.id)}>
                            Cancelar
                          </button>
                        </>
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
