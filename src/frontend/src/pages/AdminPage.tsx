import { useState, type FormEvent, type CSSProperties } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { pagamentoApi } from '../api/pagamento'
import { eventosApi } from '../api/eventos'
import { ingressosApi } from '../api/ingressos'
import { Layout } from '../components/Layout'
import type { CriarEventoRequest } from '../types'

// ─── Aba: Gerenciar Pagamentos ────────────────────────────────────

function PainelPagamentos() {
  const qc = useQueryClient()

  const { data: pagamentos = [], isLoading } = useQuery({
    queryKey: ['admin-pagamentos'],
    queryFn: () => pagamentoApi.listar(),
  })

  const { data: ingressos = [] } = useQuery({
    queryKey: ['admin-ingressos'],
    queryFn: () => ingressosApi.listar(),
  })

  const ingressoMap = Object.fromEntries(ingressos.map((i) => [i.id, i]))

  const aprovar = useMutation({
    mutationFn: pagamentoApi.aprovar,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-pagamentos'] })
      qc.invalidateQueries({ queryKey: ['admin-ingressos'] })
    },
  })

  const recusar = useMutation({
    mutationFn: pagamentoApi.recusar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }),
  })

  const estornar = useMutation({
    mutationFn: pagamentoApi.estornar,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }),
  })

  const statusColor: Record<string, string> = {
    Pendente: '#f59e0b',
    Aprovado: '#16a34a',
    Recusado: '#dc2626',
    Estornado: '#6b7280',
  }

  return (
    <div>
      <h2 style={{ margin: '0 0 20px', fontSize: 18 }}>Pagamentos</h2>
      {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
      {!isLoading && pagamentos.length === 0 && (
        <p style={{ color: '#888' }}>Nenhum pagamento registrado.</p>
      )}

      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #e5e7eb', textAlign: 'left' }}>
              <th style={th}>E-mail</th>
              <th style={th}>Ingresso</th>
              <th style={th}>Método</th>
              <th style={th}>Valor</th>
              <th style={th}>Status</th>
              <th style={th}>Ações</th>
            </tr>
          </thead>
          <tbody>
            {pagamentos.map((p) => {
              const ingresso = ingressoMap[p.ingressoId]
              return (
                <tr key={p.id} style={{ borderBottom: '1px solid #f3f4f6' }}>
                  <td style={td}>{p.emailCliente}</td>
                  <td style={td}>{ingresso?.tipoIngresso ?? '—'}</td>
                  <td style={td}>{p.metodo}</td>
                  <td style={td}>R$ {p.valor.toFixed(2).replace('.', ',')}</td>
                  <td style={td}>
                    <span style={{
                      color: statusColor[p.status] ?? '#333',
                      fontWeight: 600,
                    }}>
                      {p.status}
                    </span>
                  </td>
                  <td style={{ ...td, display: 'flex', gap: 6 }}>
                    {p.status === 'Pendente' && (
                      <>
                        <button
                          onClick={() => aprovar.mutate(p.id)}
                          disabled={aprovar.isPending}
                          style={btnGreen}
                        >
                          Aprovar
                        </button>
                        <button
                          onClick={() => recusar.mutate(p.id)}
                          disabled={recusar.isPending}
                          style={btnRed}
                        >
                          Recusar
                        </button>
                      </>
                    )}
                    {p.status === 'Aprovado' && (
                      <button
                        onClick={() => estornar.mutate(p.id)}
                        disabled={estornar.isPending}
                        style={btnGray}
                      >
                        Estornar
                      </button>
                    )}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}

// ─── Aba: Criar Evento ────────────────────────────────────────────

function CriarEventoForm() {
  const qc = useQueryClient()
  const [form, setForm] = useState<CriarEventoRequest>({
    nome: '',
    local: '',
    dataHora: '',
    capacidadeTotal: 100,
  })
  const [sucesso, setSucesso] = useState('')
  const [erro, setErro] = useState('')

  const criar = useMutation({
    mutationFn: eventosApi.criar,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['eventos'] })
      setSucesso('Evento criado com sucesso!')
      setErro('')
      setForm({ nome: '', local: '', dataHora: '', capacidadeTotal: 100 })
    },
    onError: () => setErro('Erro ao criar evento.'),
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSucesso('')
    setErro('')
    criar.mutate(form)
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <h2 style={{ margin: '0 0 20px', fontSize: 18 }}>Criar Evento</h2>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
        <label style={labelStyle}>
          Nome
          <input
            value={form.nome}
            onChange={(e) => setForm({ ...form, nome: e.target.value })}
            required
            style={inputStyle}
          />
        </label>
        <label style={labelStyle}>
          Local
          <input
            value={form.local}
            onChange={(e) => setForm({ ...form, local: e.target.value })}
            required
            style={inputStyle}
          />
        </label>
        <label style={labelStyle}>
          Data e Hora
          <input
            type="datetime-local"
            value={form.dataHora}
            onChange={(e) => setForm({ ...form, dataHora: e.target.value })}
            required
            style={inputStyle}
          />
        </label>
        <label style={labelStyle}>
          Capacidade total
          <input
            type="number"
            min={1}
            value={form.capacidadeTotal}
            onChange={(e) => setForm({ ...form, capacidadeTotal: Number(e.target.value) })}
            required
            style={inputStyle}
          />
        </label>

        {sucesso && <p style={{ color: '#16a34a', fontSize: 13 }}>{sucesso}</p>}
        {erro && <p style={{ color: '#e94560', fontSize: 13 }}>{erro}</p>}

        <button type="submit" disabled={criar.isPending} style={btnPrimary}>
          {criar.isPending ? 'Criando…' : 'Criar evento'}
        </button>
      </form>
    </div>
  )
}

// ─── Aba: Adicionar Ingressos ─────────────────────────────────────

function AdicionarIngressosForm() {
  const qc = useQueryClient()
  const { data: eventos = [] } = useQuery({
    queryKey: ['eventos'],
    queryFn: eventosApi.listar,
  })
  const [eventoId, setEventoId] = useState('')
  const [tipoIngresso, setTipoIngresso] = useState('Inteira')
  const [preco, setPreco] = useState(100)
  const [quantidade, setQuantidade] = useState(10)
  const [sucesso, setSucesso] = useState('')
  const [erro, setErro] = useState('')

  const criar = useMutation({
    mutationFn: ingressosApi.criar,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['ingressos'] })
    },
  })

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSucesso('')
    setErro('')
    try {
      for (let i = 0; i < quantidade; i++) {
        await criar.mutateAsync({ eventoId, tipoIngresso, preco })
      }
      setSucesso(`${quantidade} ingressos criados com sucesso!`)
    } catch {
      setErro('Erro ao criar ingressos.')
    }
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <h2 style={{ margin: '0 0 20px', fontSize: 18 }}>Adicionar Ingressos</h2>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
        <label style={labelStyle}>
          Evento
          <select
            value={eventoId}
            onChange={(e) => setEventoId(e.target.value)}
            required
            style={inputStyle}
          >
            <option value="">Selecione um evento</option>
            {eventos.map((ev) => (
              <option key={ev.id} value={ev.id}>
                {ev.nome} ({ev.status})
              </option>
            ))}
          </select>
        </label>

        <label style={labelStyle}>
          Tipo
          <select
            value={tipoIngresso}
            onChange={(e) => setTipoIngresso(e.target.value)}
            style={inputStyle}
          >
            <option>Inteira</option>
            <option>Meia-entrada</option>
            <option>VIP</option>
            <option>Camarote</option>
          </select>
        </label>

        <label style={labelStyle}>
          Preço (R$)
          <input
            type="number"
            min={0.01}
            step={0.01}
            value={preco}
            onChange={(e) => setPreco(Number(e.target.value))}
            required
            style={inputStyle}
          />
        </label>

        <label style={labelStyle}>
          Quantidade
          <input
            type="number"
            min={1}
            value={quantidade}
            onChange={(e) => setQuantidade(Number(e.target.value))}
            required
            style={inputStyle}
          />
        </label>

        {sucesso && <p style={{ color: '#16a34a', fontSize: 13 }}>{sucesso}</p>}
        {erro && <p style={{ color: '#e94560', fontSize: 13 }}>{erro}</p>}

        <button type="submit" disabled={criar.isPending} style={btnPrimary}>
          {criar.isPending ? 'Criando…' : 'Adicionar ingressos'}
        </button>
      </form>
    </div>
  )
}

// ─── Página Admin ─────────────────────────────────────────────────

type Aba = 'pagamentos' | 'criar-evento' | 'ingressos'

export function AdminPage() {
  const [aba, setAba] = useState<Aba>('pagamentos')

  return (
    <Layout>
      <h1 style={{ fontSize: 24, color: '#1a1a2e', marginBottom: 24 }}>Painel Administrativo</h1>

      <div style={{ display: 'flex', gap: 0, marginBottom: 28, borderBottom: '2px solid #e5e7eb' }}>
        {([
          ['pagamentos', 'Pagamentos'],
          ['criar-evento', 'Criar Evento'],
          ['ingressos', 'Ingressos'],
        ] as [Aba, string][]).map(([key, label]) => (
          <button
            key={key}
            onClick={() => setAba(key)}
            style={{
              border: 'none',
              background: 'transparent',
              padding: '10px 20px',
              cursor: 'pointer',
              fontSize: 14,
              fontWeight: aba === key ? 700 : 400,
              color: aba === key ? '#e94560' : '#555',
              borderBottom: aba === key ? '2px solid #e94560' : '2px solid transparent',
              marginBottom: -2,
            }}
          >
            {label}
          </button>
        ))}
      </div>

      {aba === 'pagamentos' && <PainelPagamentos />}
      {aba === 'criar-evento' && <CriarEventoForm />}
      {aba === 'ingressos' && <AdicionarIngressosForm />}
    </Layout>
  )
}

// ─── Styles ───────────────────────────────────────────────────────

const th: CSSProperties = {
  padding: '10px 14px',
  color: '#6b7280',
  fontWeight: 600,
  fontSize: 12,
  textTransform: 'uppercase',
  letterSpacing: '0.05em',
}

const td: CSSProperties = {
  padding: '10px 14px',
  color: '#374151',
  verticalAlign: 'middle',
}

const btnGreen: CSSProperties = {
  background: '#dcfce7',
  color: '#16a34a',
  border: 'none',
  padding: '5px 12px',
  borderRadius: 5,
  cursor: 'pointer',
  fontSize: 12,
  fontWeight: 600,
}

const btnRed: CSSProperties = {
  background: '#fee2e2',
  color: '#dc2626',
  border: 'none',
  padding: '5px 12px',
  borderRadius: 5,
  cursor: 'pointer',
  fontSize: 12,
  fontWeight: 600,
}

const btnGray: CSSProperties = {
  background: '#f3f4f6',
  color: '#6b7280',
  border: 'none',
  padding: '5px 12px',
  borderRadius: 5,
  cursor: 'pointer',
  fontSize: 12,
  fontWeight: 600,
}

const btnPrimary: CSSProperties = {
  background: '#e94560',
  color: '#fff',
  border: 'none',
  padding: '10px 20px',
  borderRadius: 6,
  fontSize: 14,
  fontWeight: 600,
  cursor: 'pointer',
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
  outline: 'none',
}
