import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { eventosApi } from '../api/eventos'
import { ingressosApi } from '../api/ingressos'
import { pagamentoApi } from '../api/pagamento'
import { useAuth } from '../contexts/AuthContext'
import { Layout } from '../components/Layout'
import type { IngressoResponse } from '../types'

// ─── Status tags ──────────────────────────────────────────────────

function StatusTag({ status }: { status: string }) {
  const cls: Record<string, string> = {
    Rascunho: 'tag tag-neutral',
    Planejado: 'tag tag-neutral',
    Publicado: 'tag tag-success',
    Cancelado: 'tag tag-danger',
    Encerrado: 'tag tag-warning',
  }
  return <span className={cls[status] ?? 'tag tag-neutral'}>{status}</span>
}

// ─── Reserva / Pagamento modal ────────────────────────────────────

function ReservarModal({ eventoId, onClose }: { eventoId: string; onClose: () => void }) {
  const qc = useQueryClient()
  const { usuario } = useAuth()
  const [etapa, setEtapa] = useState<'lista' | 'pagamento'>('lista')
  const [ingressoSelecionado, setIngressoSelecionado] = useState<IngressoResponse | null>(null)
  const [metodo, setMetodo] = useState<number>(2)
  const [erro, setErro] = useState('')

  const { data: ingressos = [], isLoading } = useQuery({
    queryKey: ['ingressos', eventoId],
    queryFn: () => ingressosApi.listar(eventoId),
  })

  const reservar = useMutation({
    mutationFn: (id: string) => ingressosApi.reservar(id),
    onSuccess: (ingresso) => {
      qc.invalidateQueries({ queryKey: ['ingressos', eventoId] })
      setIngressoSelecionado(ingresso)
      setEtapa('pagamento')
    },
    onError: () => setErro('Falha ao reservar ingresso.'),
  })

  const pagar = useMutation({
    mutationFn: () =>
      pagamentoApi.criar({
        ingressoId: ingressoSelecionado!.id,
        valor: ingressoSelecionado!.preco,
        metodo,
        emailCliente: usuario!.nomeUsuario,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ingressos', eventoId] })
      onClose()
    },
    onError: () => setErro('Falha ao criar pagamento.'),
  })

  const disponiveis = ingressos.filter((i) => i.status === 'Disponivel')

  return (
    <div className="dialog-backdrop" onClick={onClose}>
      <div className="dialog" onClick={(e) => e.stopPropagation()}>
        <button
          onClick={onClose}
          className="btn btn-icon btn-secondary"
          style={{ position: 'absolute', top: 16, right: 16 }}
        >
          <i className="ph ph-x" />
        </button>

        {etapa === 'lista' && (
          <>
            <div className="dialog-title">Escolha um ingresso</div>
            {isLoading && <p className="text-muted">Carregando…</p>}
            {!isLoading && disponiveis.length === 0 && (
              <p className="text-muted">Nenhum ingresso disponível para este evento.</p>
            )}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
              {disponiveis.map((i) => (
                <div key={i.id} className="card elev-sm" style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
                  <div>
                    <div className="card-title" style={{ fontSize: 15 }}>{i.tipoIngresso}</div>
                    <div className="card-meta">R$ {i.preco.toFixed(2).replace('.', ',')}</div>
                  </div>
                  <button
                    className="btn btn-primary"
                    onClick={() => reservar.mutate(i.id)}
                    disabled={reservar.isPending}
                  >
                    Reservar
                  </button>
                </div>
              ))}
            </div>
            {erro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>}
          </>
        )}

        {etapa === 'pagamento' && ingressoSelecionado && (
          <>
            <div className="dialog-title">Pagamento</div>
            <div className="card-kicker">
              {ingressoSelecionado.tipoIngresso} · R$ {ingressoSelecionado.preco.toFixed(2).replace('.', ',')}
            </div>

            <div className="field">
              <label>Método de pagamento</label>
              <div className="seg">
                <label className="seg-opt">
                  <input type="radio" name="pay" checked={metodo === 1} onChange={() => setMetodo(1)} />
                  <i className="ph ph-credit-card" /> Cartão
                </label>
                <label className="seg-opt">
                  <input type="radio" name="pay" checked={metodo === 2} onChange={() => setMetodo(2)} />
                  <i className="ph ph-qr-code" /> Pix
                </label>
                <label className="seg-opt">
                  <input type="radio" name="pay" checked={metodo === 3} onChange={() => setMetodo(3)} />
                  <i className="ph ph-barcode" /> Boleto
                </label>
              </div>
            </div>

            {erro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>}

            <div className="dialog-actions">
              <button className="btn btn-secondary" onClick={() => setEtapa('lista')}>
                <i className="ph ph-arrow-left" /> Voltar
              </button>
              <button
                className="btn btn-primary"
                onClick={() => pagar.mutate()}
                disabled={pagar.isPending}
              >
                {pagar.isPending ? 'Processando…' : 'Confirmar pagamento'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

// ─── Página principal ─────────────────────────────────────────────

export function EventosPage() {
  const { isAdmin } = useAuth()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [modalEventoId, setModalEventoId] = useState<string | null>(null)

  const { data: eventos = [], isLoading, isError } = useQuery({
    queryKey: ['eventos'],
    queryFn: eventosApi.listar,
  })

  const publicar = useMutation({
    mutationFn: eventosApi.publicar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['eventos'] }),
  })
  const cancelar = useMutation({
    mutationFn: eventosApi.cancelar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['eventos'] }),
  })
  const encerrar = useMutation({
    mutationFn: eventosApi.encerrar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['eventos'] }),
  })

  return (
    <Layout>
      <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <div>
          <h1 style={{ margin: '0 0 var(--space-1)' }}>Viva a música.</h1>
          <p className="text-muted" style={{ margin: 0, fontSize: 15 }}>
            Shows, festivais e stand-up perto de você. Compra rápida, ingresso na hora.
          </p>
        </div>
        {isAdmin && (
          <button className="btn btn-primary" onClick={() => navigate('/admin/eventos/novo')}>
            <i className="ph ph-plus" /> Novo evento
          </button>
        )}
      </div>

      {isLoading && <p className="text-muted">Carregando eventos…</p>}
      {isError && <p style={{ color: 'var(--color-danger)' }}>Erro ao carregar eventos.</p>}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 'var(--space-6)' }}>
        {eventos.map((evento) => (
          <div key={evento.id} className="card elev-sm" style={{ gap: 'var(--space-3)' }}>
            {/* Header */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div className="card-kicker">{evento.local}</div>
              <StatusTag status={evento.status} />
            </div>

            <div className="card-title">{evento.nome}</div>

            <div className="card-meta">
              <i className="ph ph-calendar-blank" />
              {new Date(evento.dataHora).toLocaleString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
            </div>

            <div className="card-meta">
              <i className="ph ph-users" />
              Capacidade: {evento.capacidadeTotal} pessoas
            </div>

            <div className="hr" style={{ margin: '4px 0' }} />

            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--space-2)' }}>
              {evento.status === 'Publicado' && (
                <button className="btn btn-primary" onClick={() => setModalEventoId(evento.id)}>
                  <i className="ph ph-ticket" /> Comprar ingresso
                </button>
              )}
              {isAdmin && evento.status === 'Rascunho' && (
                <button className="btn btn-secondary" onClick={() => publicar.mutate(evento.id)}>
                  Publicar
                </button>
              )}
              {isAdmin && (evento.status === 'Rascunho' || evento.status === 'Publicado') && (
                <button className="btn btn-danger" onClick={() => cancelar.mutate(evento.id)}>
                  Cancelar
                </button>
              )}
              {isAdmin && evento.status === 'Publicado' && (
                <button className="btn btn-secondary" onClick={() => encerrar.mutate(evento.id)}>
                  Encerrar
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {modalEventoId && (
        <ReservarModal eventoId={modalEventoId} onClose={() => setModalEventoId(null)} />
      )}
    </Layout>
  )
}

export { StatusTag }
export const btnPrimary = 'btn btn-primary'
export const btnSecondary = 'btn btn-secondary'
