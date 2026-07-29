import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { pagamentoApi } from '../api/pagamento'
import { Layout } from '../components/Layout'
import * as ui from '../components/ui'

const STATUS_COLOR: Record<string, string> = {
  Pendente: '#f59e0b',
  Aprovado: '#16a34a',
  Recusado: '#dc2626',
  Estornado: '#6b7280',
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

  function confirmarReembolso(id: string) {
    if (confirm('Solicitar reembolso deste pagamento?')) reembolso.mutate(id)
  }

  const total = pagamentos
    .filter((p) => p.status === 'Aprovado')
    .reduce((sum, p) => sum + p.valor, 0)

  return (
    <Layout>
      <div style={ui.pageHeader}>
        <h1 style={ui.h1}>Extrato de Pagamentos</h1>
        {pagamentos.length > 0 && (
          <div style={{ fontSize: 14, color: '#555' }}>
            Total aprovado:{' '}
            <strong style={{ color: '#16a34a' }}>
              R$ {total.toFixed(2).replace('.', ',')}
            </strong>
          </div>
        )}
      </div>

      <div style={ui.card}>
        {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
        {!isLoading && pagamentos.length === 0 && (
          <p style={{ color: '#888' }}>Nenhum pagamento registrado.</p>
        )}

        {pagamentos.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table style={ui.table}>
              <thead>
                <tr>
                  <th style={ui.th}>Método</th>
                  <th style={ui.th}>Valor</th>
                  <th style={ui.th}>Status</th>
                  <th style={ui.th}>Ações</th>
                </tr>
              </thead>
              <tbody>
                {pagamentos.map((p) => (
                  <tr key={p.id}>
                    <td style={ui.td}>{METODO_LABEL[p.metodo] ?? p.metodo}</td>
                    <td style={ui.td}>R$ {p.valor.toFixed(2).replace('.', ',')}</td>
                    <td style={ui.td}>
                      <span style={{
                        color: STATUS_COLOR[p.status] ?? '#333',
                        fontWeight: 600,
                        fontSize: 13,
                      }}>
                        {p.status}
                      </span>
                    </td>
                    <td style={ui.td}>
                      {p.status === 'Aprovado' && (
                        <button
                          style={{ ...ui.btnSmall, color: '#92400e', borderColor: '#f59e0b' }}
                          onClick={() => confirmarReembolso(p.id)}
                          disabled={reembolso.isPending}
                        >
                          Solicitar reembolso
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
