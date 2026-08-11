import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { ingressosApi } from '../../api/ingressos'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'

const statusTagCls: Record<string, string> = {
  Disponivel: 'tag tag-success',
  Reservado: 'tag tag-warning',
  Vendido: 'tag tag-accent',
  Cancelado: 'tag tag-danger',
}

export function IngressosAdminPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [eventoFiltro, setEventoFiltro] = useState('')

  const { data: eventos = [] } = useQuery({ queryKey: ['admin-eventos'], queryFn: eventosApi.listar })

  const { data: ingressos = [], isLoading } = useQuery({
    queryKey: ['admin-ingressos', eventoFiltro],
    queryFn: () => ingressosApi.listar(eventoFiltro || undefined),
  })

  const excluir = useMutation({ mutationFn: ingressosApi.excluir, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-ingressos'] }) })
  const cancelar = useMutation({ mutationFn: ingressosApi.cancelar, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-ingressos'] }) })

  const eventoMap = Object.fromEntries(eventos.map((e) => [e.id, e.nome]))

  return (
    <Layout>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--space-4)' }}>
        <h1 style={{ margin: 0 }}>Ingressos</h1>
        <button className="btn btn-primary" onClick={() => navigate('/admin/ingressos/novo')}>
          <i className="ph ph-plus" /> Novo ingresso
        </button>
      </div>

      <div style={{ marginBottom: 'var(--space-4)' }}>
        <select className="input" value={eventoFiltro} onChange={(e) => setEventoFiltro(e.target.value)} style={{ maxWidth: 320 }}>
          <option value="">Todos os eventos</option>
          {eventos.map((ev) => <option key={ev.id} value={ev.id}>{ev.nome}</option>)}
        </select>
      </div>

      <div className="card elev-sm" style={{ padding: 0, overflow: 'hidden' }}>
        {isLoading && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Carregando…</p>}
        {!isLoading && ingressos.length === 0 && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Nenhum ingresso encontrado.</p>}

        {ingressos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table className="table">
              <thead>
                <tr>
                  <th>Evento</th><th>Tipo</th><th>Preço</th><th>Status</th><th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {ingressos.map((i) => (
                  <tr key={i.id}>
                    <td>{eventoMap[i.eventoId] ?? i.eventoId.slice(0, 8)}</td>
                    <td style={{ fontWeight: 500 }}>{i.tipoIngresso}</td>
                    <td>R$ {i.preco.toFixed(2).replace('.', ',')}</td>
                    <td><span className={statusTagCls[i.status] ?? 'tag tag-neutral'}>{i.status}</span></td>
                    <td>
                      <div style={{ display: 'flex', gap: 6 }}>
                        {i.status === 'Disponivel' && (
                          <>
                            <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => navigate(`/admin/ingressos/${i.id}`)}>
                              <i className="ph ph-pencil-simple" /> Editar
                            </button>
                            <button className="btn btn-danger" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => { if (confirm('Excluir?')) excluir.mutate(i.id) }}>
                              <i className="ph ph-trash" />
                            </button>
                          </>
                        )}
                        {(i.status === 'Reservado' || i.status === 'Vendido') && (
                          <button className="btn btn-danger" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => cancelar.mutate(i.id)}>
                            Cancelar
                          </button>
                        )}
                      </div>
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
