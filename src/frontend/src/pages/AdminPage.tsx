import { useState, type FormEvent } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { pagamentoApi } from '../api/pagamento'
import { eventosApi } from '../api/eventos'
import { ingressosApi } from '../api/ingressos'
import { Layout } from '../components/Layout'
import type { CriarEventoRequest } from '../types'

type Aba = 'pagamentos' | 'criar-evento' | 'ingressos'

const pagTagCls: Record<string, string> = {
  Pendente: 'tag tag-warning',
  Aprovado: 'tag tag-success',
  Recusado: 'tag tag-danger',
  Estornado: 'tag tag-neutral',
}

function PainelPagamentos() {
  const qc = useQueryClient()
  const { data: pagamentos = [], isLoading } = useQuery({ queryKey: ['admin-pagamentos'], queryFn: () => pagamentoApi.listar() })
  const { data: ingressos = [] } = useQuery({ queryKey: ['admin-ingressos'], queryFn: () => ingressosApi.listar() })
  const ingressoMap = Object.fromEntries(ingressos.map((i) => [i.id, i]))
  const aprovar  = useMutation({ mutationFn: pagamentoApi.aprovar,  onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }) })
  const recusar  = useMutation({ mutationFn: pagamentoApi.recusar,  onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }) })
  const estornar = useMutation({ mutationFn: pagamentoApi.estornar, onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-pagamentos'] }) })

  return (
    <>
      <h2 style={{ margin: '0 0 var(--space-4)', fontSize: 20, fontFamily: 'var(--font-heading)' }}>Pagamentos</h2>
      {isLoading && <p className="text-muted">Carregando…</p>}
      {!isLoading && pagamentos.length === 0 && <p className="text-muted">Nenhum pagamento registrado.</p>}
      {pagamentos.length > 0 && (
        <div className="card elev-sm" style={{ padding: 0, overflow: 'hidden' }}>
          <div style={{ overflowX: 'auto' }}>
            <table className="table">
              <thead><tr><th>Cliente</th><th>Ingresso</th><th>Método</th><th>Valor</th><th>Status</th><th>Ações</th></tr></thead>
              <tbody>
                {pagamentos.map((p) => (
                  <tr key={p.id}>
                    <td>{p.emailCliente}</td>
                    <td>{ingressoMap[p.ingressoId]?.tipoIngresso ?? '—'}</td>
                    <td>{p.metodo}</td>
                    <td style={{ fontFamily: 'var(--font-heading)' }}>R$ {p.valor.toFixed(2).replace('.', ',')}</td>
                    <td><span className={pagTagCls[p.status] ?? 'tag tag-neutral'}>{p.status}</span></td>
                    <td>
                      <div style={{ display: 'flex', gap: 6 }}>
                        {p.status === 'Pendente' && (
                          <>
                            <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px', color: 'var(--color-success)', borderColor: 'var(--color-success)' }} onClick={() => aprovar.mutate(p.id)} disabled={aprovar.isPending}>Aprovar</button>
                            <button className="btn btn-danger" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => recusar.mutate(p.id)} disabled={recusar.isPending}>Recusar</button>
                          </>
                        )}
                        {p.status === 'Aprovado' && (
                          <button className="btn btn-secondary" style={{ fontSize: 12, padding: '4px 10px' }} onClick={() => estornar.mutate(p.id)} disabled={estornar.isPending}>Estornar</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </>
  )
}

function CriarEventoForm() {
  const qc = useQueryClient()
  const [form, setForm] = useState<CriarEventoRequest>({ nome: '', local: '', dataHora: '', capacidadeTotal: 100 })
  const [sucesso, setSucesso] = useState(''); const [erro, setErro] = useState('')

  const criar = useMutation({
    mutationFn: (data: CriarEventoRequest) => {
      const dataHoraISO = data.dataHora.length === 16 ? data.dataHora + ':00' : data.dataHora
      return eventosApi.criar({ ...data, dataHora: dataHoraISO })
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['eventos'] }); setSucesso('Evento criado!'); setErro(''); setForm({ nome: '', local: '', dataHora: '', capacidadeTotal: 100 }) },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao criar evento.')
    },
  })

  return (
    <div style={{ maxWidth: 480 }}>
      <h2 style={{ margin: '0 0 var(--space-4)', fontSize: 20, fontFamily: 'var(--font-heading)' }}>Criar Evento</h2>
      <div className="card elev-sm" style={{ gap: 16 }}>
        <form onSubmit={(e: FormEvent) => { e.preventDefault(); setSucesso(''); setErro(''); criar.mutate(form) }} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div className="field"><label>Nome</label><input className="input" value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required /></div>
          <div className="field"><label>Local</label><input className="input" value={form.local} onChange={(e) => setForm({ ...form, local: e.target.value })} required /></div>
          <div className="field"><label>Data e Hora</label><input className="input" type="datetime-local" value={form.dataHora} onChange={(e) => setForm({ ...form, dataHora: e.target.value })} required /></div>
          <div className="field"><label>Capacidade total</label><input className="input" type="number" min={1} value={form.capacidadeTotal} onChange={(e) => setForm({ ...form, capacidadeTotal: Number(e.target.value) })} required /></div>
          {sucesso && <p style={{ color: 'var(--color-success)', fontSize: 13, margin: 0 }}>{sucesso}</p>}
          {erro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>}
          <button type="submit" className="btn btn-primary" disabled={criar.isPending}>{criar.isPending ? 'Criando…' : 'Criar evento'}</button>
        </form>
      </div>
    </div>
  )
}

function AdicionarIngressosForm() {
  const qc = useQueryClient()
  const { data: eventos = [] } = useQuery({ queryKey: ['eventos'], queryFn: eventosApi.listar })
  const [eventoId, setEventoId] = useState(''); const [tipoIngresso, setTipoIngresso] = useState('Inteira')
  const [preco, setPreco] = useState(100); const [quantidade, setQuantidade] = useState(10)
  const [sucesso, setSucesso] = useState(''); const [erro, setErro] = useState('')
  const criar = useMutation({ mutationFn: ingressosApi.criar, onSuccess: () => qc.invalidateQueries({ queryKey: ['ingressos'] }) })

  async function handleSubmit(e: FormEvent) {
    e.preventDefault(); setSucesso(''); setErro('')
    try {
      for (let i = 0; i < quantidade; i++) await criar.mutateAsync({ eventoId, tipoIngresso, preco })
      setSucesso(`${quantidade} ingressos criados!`)
    } catch (err: unknown) {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao criar ingressos.')
    }
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <h2 style={{ margin: '0 0 var(--space-4)', fontSize: 20, fontFamily: 'var(--font-heading)' }}>Adicionar Ingressos</h2>
      <div className="card elev-sm" style={{ gap: 16 }}>
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div className="field"><label>Evento</label>
            <select className="input" value={eventoId} onChange={(e) => setEventoId(e.target.value)} required>
              <option value="">Selecione um evento</option>
              {eventos.map((ev) => <option key={ev.id} value={ev.id}>{ev.nome} ({ev.status})</option>)}
            </select>
          </div>
          <div className="field"><label>Tipo</label>
            <select className="input" value={tipoIngresso} onChange={(e) => setTipoIngresso(e.target.value)}>
              <option>Inteira</option><option>Meia-entrada</option><option>VIP</option><option>Camarote</option>
            </select>
          </div>
          <div className="field"><label>Preço (R$)</label><input className="input" type="number" min={0.01} step={0.01} value={preco} onChange={(e) => setPreco(Number(e.target.value))} required /></div>
          <div className="field"><label>Quantidade</label><input className="input" type="number" min={1} value={quantidade} onChange={(e) => setQuantidade(Number(e.target.value))} required /></div>
          {sucesso && <p style={{ color: 'var(--color-success)', fontSize: 13, margin: 0 }}>{sucesso}</p>}
          {erro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>}
          <button type="submit" className="btn btn-primary" disabled={criar.isPending}>{criar.isPending ? 'Criando…' : 'Adicionar ingressos'}</button>
        </form>
      </div>
    </div>
  )
}

export function AdminPage() {
  const [aba, setAba] = useState<Aba>('pagamentos')
  const abas: [Aba, string][] = [['pagamentos', 'Pagamentos'], ['criar-evento', 'Criar Evento'], ['ingressos', 'Ingressos']]

  return (
    <Layout>
      <h1 style={{ marginBottom: 'var(--space-6)' }}>Painel Administrativo</h1>

      <div style={{ display: 'flex', gap: 4, marginBottom: 'var(--space-6)', borderBottom: '1px solid var(--color-divider)' }}>
        {abas.map(([key, label]) => (
          <button key={key} onClick={() => setAba(key)} style={{
            border: 'none', background: 'transparent', cursor: 'pointer',
            padding: '8px 16px', fontSize: 14, fontFamily: 'var(--font-heading)',
            color: aba === key ? 'var(--color-accent)' : 'color-mix(in srgb, var(--color-text) 60%, transparent)',
            borderBottom: aba === key ? '2px solid var(--color-accent)' : '2px solid transparent',
            marginBottom: -1,
          }}>
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
