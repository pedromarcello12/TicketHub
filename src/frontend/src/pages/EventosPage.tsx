import { useState, type CSSProperties } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { eventosApi } from '../api/eventos'
import { ingressosApi } from '../api/ingressos'
import { pagamentoApi } from '../api/pagamento'
import { useAuth } from '../contexts/AuthContext'
import { Layout } from '../components/Layout'
import type { IngressoResponse } from '../types'

// ─── Status badge colors ──────────────────────────────────────────

const statusColor: Record<string, string> = {
  Rascunho: '#6b7280',
  Publicado: '#16a34a',
  Cancelado: '#dc2626',
  Encerrado: '#92400e',
}

// ─── Reserva / Pagamento modal ────────────────────────────────────

interface ReservarModalProps {
  eventoId: string
  onClose: () => void
}

function ReservarModal({ eventoId, onClose }: ReservarModalProps) {
  const qc = useQueryClient()
  const { usuario } = useAuth()
  const [etapa, setEtapa] = useState<'lista' | 'pagamento'>('lista')
  const [ingressoSelecionado, setIngressoSelecionado] = useState<IngressoResponse | null>(null)
  const [metodo, setMetodo] = useState<number>(2) // Pix default
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
        emailCliente: usuario!.email,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ingressos', eventoId] })
      onClose()
    },
    onError: () => setErro('Falha ao criar pagamento.'),
  })

  const disponiveis = ingressos.filter((i) => i.status === 'Disponivel')

  return (
    <div style={overlay} onClick={onClose}>
      <div style={modalBox} onClick={(e) => e.stopPropagation()}>
        <button onClick={onClose} style={closeBtn}>✕</button>

        {etapa === 'lista' && (
          <>
            <h2 style={{ margin: '0 0 16px', fontSize: 18 }}>Escolha um ingresso</h2>
            {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
            {!isLoading && disponiveis.length === 0 && (
              <p style={{ color: '#888' }}>Nenhum ingresso disponível para este evento.</p>
            )}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {disponiveis.map((i) => (
                <div key={i.id} style={ingressoCard}>
                  <div>
                    <strong>{i.tipoIngresso}</strong>
                    <div style={{ color: '#666', fontSize: 13 }}>
                      R$ {i.preco.toFixed(2).replace('.', ',')}
                    </div>
                  </div>
                  <button
                    onClick={() => reservar.mutate(i.id)}
                    disabled={reservar.isPending}
                    style={btnPrimary}
                  >
                    Reservar
                  </button>
                </div>
              ))}
            </div>
          </>
        )}

        {etapa === 'pagamento' && ingressoSelecionado && (
          <>
            <h2 style={{ margin: '0 0 4px', fontSize: 18 }}>Pagamento</h2>
            <p style={{ color: '#555', fontSize: 13, margin: '0 0 20px' }}>
              {ingressoSelecionado.tipoIngresso} · R${' '}
              {ingressoSelecionado.preco.toFixed(2).replace('.', ',')}
            </p>

            <label style={labelStyle}>
              Método de pagamento
              <select
                value={metodo}
                onChange={(e) => setMetodo(Number(e.target.value))}
                style={inputStyle}
              >
                <option value={2}>Pix</option>
                <option value={1}>Cartão de Crédito</option>
                <option value={3}>Boleto</option>
              </select>
            </label>

            {erro && <p style={{ color: '#e94560', fontSize: 13 }}>{erro}</p>}

            <div style={{ display: 'flex', gap: 10, marginTop: 20 }}>
              <button onClick={() => setEtapa('lista')} style={btnSecondary}>
                Voltar
              </button>
              <button
                onClick={() => pagar.mutate()}
                disabled={pagar.isPending}
                style={{ ...btnPrimary, flex: 1 }}
              >
                {pagar.isPending ? 'Processando…' : 'Confirmar pagamento'}
              </button>
            </div>
          </>
        )}

        {erro && etapa === 'lista' && (
          <p style={{ color: '#e94560', fontSize: 13, marginTop: 8 }}>{erro}</p>
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
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 28 }}>
        <h1 style={{ margin: 0, fontSize: 24, color: '#1a1a2e' }}>Eventos</h1>
        {isAdmin && (
          <button onClick={() => navigate('/admin/criar-evento')} style={btnPrimary}>
            + Novo evento
          </button>
        )}
      </div>

      {isLoading && <p style={{ color: '#888' }}>Carregando eventos…</p>}
      {isError && <p style={{ color: '#e94560' }}>Erro ao carregar eventos.</p>}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: 20 }}>
        {eventos.map((evento) => (
          <div key={evento.id} style={eventoCard}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
              <h2 style={{ margin: 0, fontSize: 17, color: '#1a1a2e' }}>{evento.nome}</h2>
              <span style={{
                background: statusColor[evento.status] ?? '#6b7280',
                color: '#fff',
                padding: '2px 10px',
                borderRadius: 20,
                fontSize: 11,
                fontWeight: 600,
                whiteSpace: 'nowrap',
                marginLeft: 8,
              }}>
                {evento.status}
              </span>
            </div>

            <p style={{ margin: '4px 0', color: '#555', fontSize: 13 }}>📍 {evento.local}</p>
            <p style={{ margin: '4px 0 16px', color: '#555', fontSize: 13 }}>
              🗓 {new Date(evento.dataHora).toLocaleString('pt-BR')}
            </p>
            <p style={{ margin: '0 0 16px', color: '#888', fontSize: 12 }}>
              Capacidade: {evento.capacidadeTotal} pessoas
            </p>

            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {evento.status === 'Publicado' && (
                <button onClick={() => setModalEventoId(evento.id)} style={btnPrimary}>
                  Comprar ingresso
                </button>
              )}

              {isAdmin && evento.status === 'Rascunho' && (
                <button onClick={() => publicar.mutate(evento.id)} style={btnSecondary}>
                  Publicar
                </button>
              )}
              {isAdmin && (evento.status === 'Rascunho' || evento.status === 'Publicado') && (
                <button onClick={() => cancelar.mutate(evento.id)} style={btnDanger}>
                  Cancelar
                </button>
              )}
              {isAdmin && evento.status === 'Publicado' && (
                <button onClick={() => encerrar.mutate(evento.id)} style={btnSecondary}>
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

// ─── Styles ───────────────────────────────────────────────────────

const eventoCard: CSSProperties = {
  background: '#fff',
  borderRadius: 10,
  padding: 20,
  boxShadow: '0 2px 8px rgba(0,0,0,.08)',
  border: '1px solid #e5e7eb',
}

const ingressoCard: CSSProperties = {
  background: '#f9fafb',
  border: '1px solid #e5e7eb',
  borderRadius: 8,
  padding: '12px 16px',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
}

export const btnPrimary: CSSProperties = {
  background: '#e94560',
  color: '#fff',
  border: 'none',
  padding: '8px 16px',
  borderRadius: 6,
  fontSize: 13,
  fontWeight: 600,
  cursor: 'pointer',
}

export const btnSecondary: CSSProperties = {
  background: '#fff',
  color: '#1a1a2e',
  border: '1px solid #d1d5db',
  padding: '8px 16px',
  borderRadius: 6,
  fontSize: 13,
  cursor: 'pointer',
}

const btnDanger: CSSProperties = {
  background: '#fff',
  color: '#dc2626',
  border: '1px solid #dc2626',
  padding: '8px 16px',
  borderRadius: 6,
  fontSize: 13,
  cursor: 'pointer',
}

const overlay: CSSProperties = {
  position: 'fixed',
  inset: 0,
  background: 'rgba(0,0,0,.5)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  zIndex: 100,
}

const modalBox: CSSProperties = {
  background: '#fff',
  borderRadius: 12,
  padding: '32px 28px',
  width: '100%',
  maxWidth: 440,
  position: 'relative',
  maxHeight: '90vh',
  overflowY: 'auto',
}

const closeBtn: CSSProperties = {
  position: 'absolute',
  top: 16,
  right: 16,
  background: 'transparent',
  border: 'none',
  fontSize: 18,
  cursor: 'pointer',
  color: '#888',
}

const labelStyle: CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: 6,
  fontSize: 14,
  color: '#333',
  fontWeight: 500,
}

const inputStyle: CSSProperties = {
  border: '1px solid #d1d5db',
  borderRadius: 6,
  padding: '9px 12px',
  fontSize: 14,
  background: '#fff',
}
