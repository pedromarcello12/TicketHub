import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { pagamentoApi } from '../api/pagamento'
import { Layout } from '../components/Layout'

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

export function ExtratoPage() {
  const qc = useQueryClient()

  const { data: pagamentos = [], isLoading } = useQuery({
    queryKey: ['meus-pagamentos'],
    queryFn: () => pagamentoApi.listar(),
  })

  const reembolso = useMutation({
    mutationFn: pagamentoApi.estornar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['meus-pagamentos'] }),
  })

  const total = pagamentos.filter((p) => p.status === 'Aprovado').reduce((sum, p) => sum + p.valor, 0)

  function confirmarReembolso(id: string) {
    if (confirm('Solicitar reembolso deste pagamento?')) reembolso.mutate(id)
  }

  return (
    <Layout>
      <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <h1 style={{ margin: 0 }}>Extrato</h1>
        {pagamentos.length > 0 && (
          <div style={{ fontSize: 14 }}>
            <span className="text-muted">Total aprovado: </span>
            <strong style={{ color: 'var(--color-success)' }}>
              R$ {total.toFixed(2).replace('.', ',')}
            </strong>
          </div>
        )}
      </div>

      <div className="card elev-sm" style={{ padding: 0, overflow: 'hidden' }}>
        {isLoading && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Carregando…</p>}
        {!isLoading && pagamentos.length === 0 && (
          <div style={{ padding: 'var(--space-6)', textAlign: 'center' }}>
            <i className="ph ph-receipt" style={{ fontSize: 32, color: 'var(--color-accent)', marginBottom: 8, display: 'block' }} />
            <p className="text-muted" style={{ margin: 0 }}>Nenhum pagamento registrado.</p>
          </div>
        )}

        {pagamentos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table className="table">
              <thead>
                <tr>
                  <th>Método</th>
                  <th>Valor</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {pagamentos.map((p) => (
                  <tr key={p.id}>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                        <i className={p.metodo === 'Pix' ? 'ph ph-qr-code' : p.metodo === 'Boleto' ? 'ph ph-barcode' : 'ph ph-credit-card'} />
                        {METODO_LABEL[p.metodo] ?? p.metodo}
                      </div>
                    </td>
                    <td style={{ fontFamily: 'var(--font-heading)' }}>
                      R$ {p.valor.toFixed(2).replace('.', ',')}
                    </td>
                    <td>
                      <span className={pagTagCls[p.status] ?? 'tag tag-neutral'}>{p.status}</span>
                    </td>
                    <td>
                      {p.status === 'Aprovado' && (
                        <button
                          className="btn btn-danger"
                          style={{ fontSize: 12, padding: '3px 10px' }}
                          onClick={() => confirmarReembolso(p.id)}
                          disabled={reembolso.isPending}
                        >
                          Reembolso
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
