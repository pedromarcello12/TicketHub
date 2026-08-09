import { useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { eventosApi } from '../../api/eventos'
import type { EventoResponse } from '../../types'
import { Layout } from '../../components/Layout'

export function EventoFormPage() {
  const { id } = useParams<{ id: string }>()
  const isEdicao = !!id
  const navigate = useNavigate()
  const qc = useQueryClient()

  const [nome, setNome] = useState('')
  const [local, setLocal] = useState('')
  const [dataHora, setDataHora] = useState('')
  const [capacidadeTotal, setCapacidadeTotal] = useState(100)
  const [erro, setErro] = useState('')
  const [carregado, setCarregado] = useState(false)

  useQuery({
    queryKey: ['evento', id],
    queryFn: () => eventosApi.obterPorId(id!),
    enabled: isEdicao,
    onSuccess: (ev: EventoResponse) => {
      if (carregado) return
      setNome(ev.nome); setLocal(ev.local)
      setDataHora(new Date(ev.dataHora).toISOString().slice(0, 16))
      setCapacidadeTotal(ev.capacidadeTotal)
      setCarregado(true)
    },
  } as Parameters<typeof useQuery>[0])

  const criar = useMutation({
    mutationFn: eventosApi.criar,
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-eventos'] }); navigate('/admin/eventos') },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar evento.')
    },
  })

  const atualizar = useMutation({
    mutationFn: (data: Parameters<typeof eventosApi.atualizar>[1]) => eventosApi.atualizar(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-eventos'] })
      qc.invalidateQueries({ queryKey: ['evento', id] })
      navigate('/admin/eventos')
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar evento.')
    },
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault(); setErro('')
    const dataHoraISO = dataHora.length === 16 ? dataHora + ':00' : dataHora
    const payload = { nome, local, dataHora: dataHoraISO, capacidadeTotal }
    if (isEdicao) atualizar.mutate(payload); else criar.mutate(payload)
  }

  const isPending = criar.isPending || atualizar.isPending

  return (
    <Layout>
      <div style={{ maxWidth: 560 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 'var(--space-6)' }}>
          <button className="btn btn-ghost" onClick={() => navigate('/admin/eventos')}>
            <i className="ph ph-arrow-left" /> Voltar
          </button>
          <h1 style={{ margin: 0 }}>{isEdicao ? 'Editar evento' : 'Novo evento'}</h1>
        </div>

        <div className="card elev-sm" style={{ gap: 16 }}>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            <div className="field">
              <label>Nome</label>
              <input className="input" value={nome} onChange={(e) => setNome(e.target.value)} required />
            </div>
            <div className="field">
              <label>Local</label>
              <input className="input" value={local} onChange={(e) => setLocal(e.target.value)} required />
            </div>
            <div className="field">
              <label>Data e Hora</label>
              <input className="input" type="datetime-local" value={dataHora} onChange={(e) => setDataHora(e.target.value)} required />
            </div>
            <div className="field">
              <label>Capacidade total</label>
              <input className="input" type="number" min={1} value={capacidadeTotal} onChange={(e) => setCapacidadeTotal(Number(e.target.value))} required />
            </div>

            {erro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>}

            <div style={{ display: 'flex', gap: 10 }}>
              <button type="button" className="btn btn-secondary" onClick={() => navigate('/admin/eventos')}>
                Cancelar
              </button>
              <button type="submit" className="btn btn-primary" disabled={isPending} style={{ flex: 1 }}>
                {isPending ? 'Salvando…' : isEdicao ? 'Salvar alterações' : 'Criar evento'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  )
}
