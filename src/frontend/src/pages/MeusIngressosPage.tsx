import { useQuery } from '@tanstack/react-query'
import type { CSSProperties } from 'react'
import { ingressosApi } from '../api/ingressos'
import { pagamentoApi } from '../api/pagamento'
import { Layout } from '../components/Layout'

const statusColor: Record<string, { bg: string; color: string }> = {
  Disponivel: { bg: '#dcfce7', color: '#16a34a' },
  Reservado: { bg: '#fef9c3', color: '#92400e' },
  Vendido: { bg: '#dbeafe', color: '#1d4ed8' },
  Cancelado: { bg: '#fee2e2', color: '#dc2626' },
}

const pagStatusColor: Record<string, { bg: string; color: string }> = {
  Pendente: { bg: '#fef9c3', color: '#92400e' },
  Aprovado: { bg: '#dcfce7', color: '#16a34a' },
  Recusado: { bg: '#fee2e2', color: '#dc2626' },
  Estornado: { bg: '#f3f4f6', color: '#6b7280' },
}

export function MeusIngressosPage() {
  const { data: ingressos = [], isLoading } = useQuery({
    queryKey: ['meus-ingressos'],
    queryFn: () => ingressosApi.listar(),
  })

  const { data: pagamentos = [] } = useQuery({
    queryKey: ['meus-pagamentos'],
    queryFn: () => pagamentoApi.listar(),
  })

  const pagamentoPorIngresso = Object.fromEntries(
    pagamentos.map((p) => [p.ingressoId, p])
  )

  const meuIngressos = ingressos.filter((i) => i.status !== 'Disponivel')

  return (
    <Layout>
      <h1 style={{ fontSize: 24, color: '#1a1a2e', marginBottom: 28 }}>Meus Ingressos</h1>

      {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
      {!isLoading && meuIngressos.length === 0 && (
        <p style={{ color: '#888' }}>Você ainda não possui ingressos reservados ou comprados.</p>
      )}

      <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
        {meuIngressos.map((ingresso) => {
          const pag = pagamentoPorIngresso[ingresso.id]
          const sc = statusColor[ingresso.status] ?? { bg: '#f3f4f6', color: '#6b7280' }

          return (
            <div key={ingresso.id} style={card}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <strong style={{ fontSize: 15 }}>{ingresso.tipoIngresso}</strong>
                  <div style={{ color: '#555', fontSize: 13, marginTop: 2 }}>
                    R$ {ingresso.preco.toFixed(2).replace('.', ',')}
                  </div>
                  {ingresso.reservadoAte && (
                    <div style={{ color: '#f59e0b', fontSize: 12, marginTop: 2 }}>
                      Reservado até: {new Date(ingresso.reservadoAte).toLocaleTimeString('pt-BR')}
                    </div>
                  )}
                </div>
                <span style={{
                  background: sc.bg,
                  color: sc.color,
                  padding: '3px 12px',
                  borderRadius: 20,
                  fontSize: 12,
                  fontWeight: 600,
                }}>
                  {ingresso.status}
                </span>
              </div>

              {pag && (
                <div style={{ marginTop: 12, padding: '10px 14px', background: '#f9fafb', borderRadius: 8, border: '1px solid #e5e7eb' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div style={{ fontSize: 13, color: '#444' }}>
                      Pagamento via <strong>{pag.metodo}</strong>
                    </div>
                    <span style={{
                      background: (pagStatusColor[pag.status] ?? { bg: '#f3f4f6', color: '#6b7280' }).bg,
                      color: (pagStatusColor[pag.status] ?? { bg: '#f3f4f6', color: '#6b7280' }).color,
                      padding: '2px 10px',
                      borderRadius: 20,
                      fontSize: 11,
                      fontWeight: 600,
                    }}>
                      {pag.status}
                    </span>
                  </div>
                </div>
              )}
            </div>
          )
        })}
      </div>
    </Layout>
  )
}

const card: CSSProperties = {
  background: '#fff',
  borderRadius: 10,
  padding: '16px 20px',
  boxShadow: '0 2px 8px rgba(0,0,0,.07)',
  border: '1px solid #e5e7eb',
}
