import { useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ingressosApi } from '../../api/ingressos'
import type { IngressoResponse } from '../../types'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'

export function IngressoFormPage() {
  const { id } = useParams<{ id: string }>()
  const isEdicao = !!id
  const navigate = useNavigate()
  const qc = useQueryClient()

  const [eventoId, setEventoId] = useState('')
  const [tipoIngresso, setTipoIngresso] = useState('Inteira')
  const [preco, setPreco] = useState(100)
  const [erro, setErro] = useState('')
  const [carregado, setCarregado] = useState(false)

  const { data: eventos = [] } = useQuery({ queryKey: ['admin-eventos'], queryFn: eventosApi.listar })

  useQuery({
    queryKey: ['ingresso', id],
    queryFn: () => ingressosApi.obterPorId(id!),
    enabled: isEdicao,
    onSuccess: (i: IngressoResponse) => {
      if (carregado) return
      setEventoId(i.eventoId); setTipoIngresso(i.tipoIngresso); setPreco(i.preco)
      setCarregado(true)
    },
  } as Parameters<typeof useQuery>[0])

  const criar = useMutation({
    mutationFn: ingressosApi.criar,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-ingressos'] }); navigate('/admin/ingressos') },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar ingresso.')
    },
  })

  const atualizar = useMutation({
    mutationFn: (data: { tipoIngresso: string; preco: number }) => ingressosApi.atualizar(id!, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-ingressos'] }); navigate('/admin/ingressos') },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar ingresso.')
    },
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault(); setErro('')
    if (isEdicao) atualizar.mutate({ tipoIngresso, preco })
    else criar.mutate({ eventoId, tipoIngresso, preco })
  }

  const isPending = criar.isPending || atualizar.isPending

  return (
    <Layout>
      <div style={{ maxWidth: 480 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 'var(--space-6)' }}>
          <button className="btn btn-ghost" onClick={() => navigate('/admin/ingressos')}>
            <i className="ph ph-arrow-left" /> Voltar
          </button>
          <h1 style={{ margin: 0 }}>{isEdicao ? 'Editar ingresso' : 'Novo ingresso'}</h1>
        </div>

        <div className="card elev-sm" style={{ gap: 16 }}>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {!isEdicao && (
              <div className="field">
                <label>Evento</label>
                <select className="input" value={eventoId} onChange={(e) => setEventoId(e.target.value)} required>
                  <option value="">Selecione um evento</option>
                  {eventos.map((ev) => <option key={ev.id} value={ev.id}>{ev.nome} ({ev.status})</option>)}
                </select>
              </div>
            )}
            <div className="field">
              <label>Tipo</label>
              <select className="input" value={tipoIngresso} onChange={(e) => setTipoIngresso(e.target.value)}>
                <option>Inteira</option>
                <option>Meia-entrada</option>
                <option>VIP</option>
                <option>Camarote</option>
              </select>
            </div>
            <div className="field">
              <label>Preço (R$)</label>
              <input className="input" type="number" min={0} step={0.01} value={preco} onChange={(e) => setPreco(Number(e.target.value))} required />
            </div>

            {erro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>}

            <div style={{ display: 'flex', gap: 10 }}>
              <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/ingressos')}>Cancelar</button>
              <button type="submit" className="btn btn-primary" disabled={isPending} style={{ flex: 1 }}>
                {isPending ? 'Salvando…' : isEdicao ? 'Salvar alterações' : 'Criar ingresso'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  )
}
