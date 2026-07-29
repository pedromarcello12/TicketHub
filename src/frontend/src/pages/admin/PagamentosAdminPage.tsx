import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { pagamentoApi } from '../../api/pagamento'
import { Layout } from '../../components/Layout'
import * as ui from '../../components/ui'

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

export function PagamentosAdminPage() {
  const qc = useQueryClient()

  const { data: pagamentos = [], isLoading } = useQuery({
    queryKey: ['admin-pagamentos'],
    queryFn: () => pagamentoApi.listar(),
  })

  const aprovar = useMutation({
    mutationFn: pagamentoApi.aprovar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }),
  })

  const recusar = useMutation({
    mutationFn: pagamentoApi.recusar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }),
  })

  const estornar = useMutation({
    mutationFn: pagamentoApi.estornar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }),
  })

  return (
    <Layout>
      <div style={ui.pageHeader}>
        <h1 style={ui.h1}>Pagamentos</h1>
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
                  <th style={ui.th}>Cliente</th>
                  <th style={ui.th}>Método</th>
                  <th style={ui.th}>Valor</th>
                  <th style={ui.th}>Status</th>
                  <th style={ui.th}>Ações</th>
                </tr>
              </thead>
              <tbody>
                {pagamentos.map((p) => (
                  <tr key={p.id}>
                    <td style={ui.td}>{p.emailCliente}</td>
                    <td style={ui.td}>{METODO_LABEL[p.metodo] ?? p.metodo}</td>
                    <td style={ui.td}>R$ {p.valor.toFixed(2).replace('.', ',')}</td>
                    <td style={ui.td}>
                      <span style={{
                        color: STATUS_COLOR[p.status] ?? '#333',
                        fontWeight: 600,
                      }}>
                        {p.status}
                      </span>
                    </td>
                    <td style={{ ...ui.td, display: 'flex', gap: 6 }}>
                      {p.status === 'Pendente' && (
                        <>
                          <button
                            style={{ ...ui.btnSmall, background: '#dcfce7', color: '#16a34a' }}
                            onClick={() => aprovar.mutate(p.id)}
                            disabled={aprovar.isPending}
                          >
                            Aprovar
                          </button>
                          <button
                            style={ui.btnDanger}
                            onClick={() => recusar.mutate(p.id)}
                            disabled={recusar.isPending}
                          >
                            Recusar
                          </button>
                        </>
                      )}
                      {p.status === 'Aprovado' && (
                        <button
                          style={ui.btnSmall}
                          onClick={() => estornar.mutate(p.id)}
                          disabled={estornar.isPending}
                        >
                          Estornar
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
