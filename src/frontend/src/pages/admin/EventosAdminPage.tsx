import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'

const statusTagCls: Record<string, string> = {
  Planejado: 'tag tag-neutral',
  Publicado: 'tag tag-success',
  Cancelado: 'tag tag-danger',
  Encerrado: 'tag tag-warning',
}

export function EventosAdminPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { data: eventos = [], isLoading } = useQuery({
    queryKey: ['admin-eventos'],
    queryFn: eventosApi.listar,
  })

  const publicar = useMutation({ mutationFn: eventosApi.publicar, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }) })
  const cancelar = useMutation({ mutationFn: eventosApi.cancelar, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }) })
  const encerrar = useMutation({ mutationFn: eventosApi.encerrar, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }) })
  const excluir  = useMutation({ mutationFn: eventosApi.excluir,  onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-eventos'] }) })

  function confirmarExcluir(id: string, nome: string) {
    if (confirm(`Excluir evento "${nome}"?`)) excluir.mutate(id)
  }

  return (
    <Layout>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <h1 style={{ margin: 0 }}>Eventos</h1>
        <button className="btn btn-primary" onClick={() => navigate('/admin/eventos/novo')}>
          <i className="ph ph-plus" /> Novo evento
        </button>
      </div>

      <div className="card elev-sm" style={{ padding: 0, overflow: 'hidden' }}>
        {isLoading && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Carregando…</p>}
        {!isLoading && eventos.length === 0 && (
          <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Nenhum evento cadastrado.</p>
        )}
        {eventos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table className="table">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Local</th>
                  <th>Data / Hora</th>
                  <th>Capacidade</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {eventos.map((ev) => (
                  <tr key={ev.id}>
                    <td style={{ fontWeight: 500 }}>{ev.nome}</td>
                    <td className="text-muted">{ev.local}</td>
                    <td>{new Date(ev.dataHora).toLocaleString('pt-BR')}</td>
                    <td>{ev.capacidadeTotal}</td>
                    <td><span className={statusTagCls[ev.status] ?? 'tag tag-neutral'}>{ev.status}</span></td>
                    <td>
                      <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                        {ev.status === 'Planejado' && (
                          <>
                            <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => navigate(`/admin/eventos/${ev.id}`)}>
                              <i className="ph ph-pencil-simple" /> Editar
                            </button>
                            <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => publicar.mutate(ev.id)}>
                              Publicar
                            </button>
                            <button className="btn btn-danger" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => confirmarExcluir(ev.id, ev.nome)}>
                              <i className="ph ph-trash" />
                            </button>
                          </>
                        )}
                        {ev.status === 'Publicado' && (
                          <>
                            <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => encerrar.mutate(ev.id)}>
                              Encerrar
                            </button>
                            <button className="btn btn-danger" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => cancelar.mutate(ev.id)}>
                              Cancelar
                            </button>
                          </>
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
