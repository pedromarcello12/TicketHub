import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { pagamentoApi } from '../../api/pagamento'
import { Layout } from '../../components/Layout'

const pagTagCls: Record<string, string> = {
  Pendente: 'tag tag-warning',
  Aprovado: 'tag tag-success',
  Recusado: 'tag tag-danger',
  Estornado: 'tag tag-neutral',
}

const METODO_LABEL: Record<string, string> = {
  CartaoCredito: 'Cartão de Crédito',
  Pix: 'Pix',
  Boleto: 'Boleto',
}

export function PagamentosAdminPage() {
  const qc = useQueryClient()

  const { data: pagamentos = [], isLoading } = useQuery({
    queryKey: ['admin-pagamentos'],
    queryFn: () => pagamentoApi.listar(),
  })

  const aprovar  = useMutation({ mutationFn: pagamentoApi.aprovar,  onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }) })
  const recusar  = useMutation({ mutationFn: pagamentoApi.recusar,  onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }) })
  const estornar = useMutation({ mutationFn: pagamentoApi.estornar, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }) })

  return (
    <Layout>
      <h1 style={{ marginBottom: 'var(--space-6)' }}>Pagamentos</h1>

      <div className="card elev-sm" style={{ padding: 0, overflow: 'hidden' }}>
        {isLoading && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Carregando…</p>}
        {!isLoading && pagamentos.length === 0 && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Nenhum pagamento registrado.</p>}

        {pagamentos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table className="table">
              <thead>
                <tr>
                  <th>Cliente</th><th>Método</th><th>Valor</th><th>Status</th><th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {pagamentos.map((p) => (
                  <tr key={p.id}>
                    <td>{p.emailCliente}</td>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                        <i className={p.metodo === 'Pix' ? 'ph ph-qr-code' : p.metodo === 'Boleto' ? 'ph ph-barcode' : 'ph ph-credit-card'} />
                        {METODO_LABEL[p.metodo] ?? p.metodo}
                      </div>
                    </td>
                    <td style={{ fontFamily: 'var(--font-heading)' }}>R$ {p.valor.toFixed(2).replace('.', ',')}</td>
                    <td><span className={pagTagCls[p.status] ?? 'tag tag-neutral'}>{p.status}</span></td>
                    <td>
                      <div style={{ display: 'flex', gap: 6 }}>
                        {p.status === 'Pendente' && (
                          <>
                            <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px', color: 'var(--color-success)', borderColor: 'var(--color-success)' }} onClick={() => aprovar.mutate(p.id)} disabled={aprovar.isPending}>
                              Aprovar
                            </button>
                            <button className="btn btn-danger" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => recusar.mutate(p.id)} disabled={recusar.isPending}>
                              Recusar
                            </button>
                          </>
                        )}
                        {p.status === 'Aprovado' && (
                          <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => estornar.mutate(p.id)} disabled={estornar.isPending}>
                            Estornar
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
