import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ingressosApi } from '../api/ingressos'
import { pagamentoApi } from '../api/pagamento'
import { Layout } from '../components/Layout'

const ingressoTagCls: Record<string, string> = {
  Disponivel: 'tag tag-neutral',
  Reservado: 'tag tag-warning',
  Vendido: 'tag tag-accent',
  Cancelado: 'tag tag-danger',
}

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

export function MeusIngressosPage() {
  const qc = useQueryClient()

  const { data: ingressos = [], isLoading } = useQuery({
    queryKey: ['meus-ingressos'],
    queryFn: () => ingressosApi.listar(),
  })

  const { data: pagamentos = [] } = useQuery({
    queryKey: ['meus-pagamentos'],
    queryFn: () => pagamentoApi.listar(),
  })

  const reembolso = useMutation({
    mutationFn: pagamentoApi.estornar,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['meus-pagamentos'] })
      qc.invalidateQueries({ queryKey: ['meus-ingressos'] })
    },
  })

  const pagamentoPorIngresso = Object.fromEntries(pagamentos.map((p) => [p.ingressoId, p]))
  const meusIngressos = ingressos.filter((i) => i.status !== 'Disponivel')

  function confirmarReembolso(pagamentoId: string) {
    if (confirm('Solicitar reembolso? O pagamento será estornado e o ingresso cancelado.')) {
      reembolso.mutate(pagamentoId)
    }
  }

  return (
    <Layout>
      <h1 style={{ marginBottom: 'var(--space-6)' }}>Meus ingressos</h1>

      {isLoading && <p className="text-muted">Carregando…</p>}

      {!isLoading && meusIngressos.length === 0 && (
        <div className="card elev-sm" style={{ maxWidth: 420, alignItems: 'flex-start' }}>
          <i className="ph ph-ticket" style={{ fontSize: 32, color: 'var(--color-accent)' }} />
          <div className="card-title">Nenhum ingresso ainda</div>
          <p className="card-body">Explore os eventos e garanta o seu.</p>
          <a href="/" className="btn btn-secondary">Ver eventos</a>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: 'var(--space-6)' }}>
        {meusIngressos.map((ingresso) => {
          const pag = pagamentoPorIngresso[ingresso.id]

          return (
            <div key={ingresso.id} className="card elev-sm" style={{ gap: 'var(--space-3)' }}>
              {/* Header row */}
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <div className="card-title" style={{ fontSize: 15 }}>{ingresso.tipoIngresso}</div>
                  <div className="card-meta" style={{ marginTop: 4 }}>
                    R$ {ingresso.preco.toFixed(2).replace('.', ',')}
                  </div>
                  {ingresso.reservadoAte && ingresso.status === 'Reservado' && (
                    <div style={{ color: 'var(--color-warning)', fontSize: 12, marginTop: 2 }}>
                      <i className="ph ph-clock" /> Reservado até: {new Date(ingresso.reservadoAte).toLocaleTimeString('pt-BR')}
                    </div>
                  )}
                </div>
                <span className={ingressoTagCls[ingresso.status] ?? 'tag tag-neutral'}>
                  {ingresso.status}
                </span>
              </div>

              {/* Payment row */}
              {pag && (
                <>
                  <div className="hr" style={{ margin: '4px 0' }} />
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
                    <div style={{ fontSize: 13 }}>
                      <span className="text-muted">Pagamento via </span>
                      <strong>{METODO_LABEL[pag.metodo] ?? pag.metodo}</strong>
                      <span className="text-muted"> · R$ {pag.valor.toFixed(2).replace('.', ',')}</span>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <span className={pagTagCls[pag.status] ?? 'tag tag-neutral'}>{pag.status}</span>
                      {pag.status === 'Aprovado' && (
                        <button
                          className="btn btn-danger"
                          style={{ fontSize: 12, padding: '3px 10px' }}
                          onClick={() => confirmarReembolso(pag.id)}
                          disabled={reembolso.isPending}
                        >
                          Reembolso
                        </button>
                      )}
                    </div>
                  </div>
                </>
              )}
            </div>
          )
        })}
      </div>
    </Layout>
  )
}
