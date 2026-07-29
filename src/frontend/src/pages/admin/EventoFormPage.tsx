import { useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'
import * as ui from '../../components/ui'

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

  // Carrega dados existentes em modo edição
  useQuery({
    queryKey: ['evento', id],
    queryFn: () => eventosApi.obterPorId(id!),
    enabled: isEdicao,
    onSuccess: (ev) => {
      if (carregado) return
      setNome(ev.nome)
      setLocal(ev.local)
      setDataHora(new Date(ev.dataHora).toISOString().slice(0, 16))
      setCapacidadeTotal(ev.capacidadeTotal)
      setCarregado(true)
    },
  } as Parameters<typeof useQuery>[0])

  const criar = useMutation({
    mutationFn: eventosApi.criar,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-eventos'] })
      navigate('/admin/eventos')
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar evento.')
    },
  })

  const atualizar = useMutation({
    mutationFn: (data: Parameters<typeof eventosApi.atualizar>[1]) =>
      eventosApi.atualizar(id!, data),
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
    e.preventDefault()
    setErro('')
    const dataHoraISO = dataHora.length === 16 ? dataHora + ':00' : dataHora
    const payload = { nome, local, dataHora: dataHoraISO, capacidadeTotal }
    if (isEdicao) atualizar.mutate(payload)
    else criar.mutate(payload)
  }

  const isPending = criar.isPending || atualizar.isPending

  return (
    <Layout>
      <div style={{ maxWidth: 560 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 28 }}>
          <button style={ui.btnSecondary} onClick={() => navigate('/admin/eventos')}>
            ← Voltar
          </button>
          <h1 style={ui.h1}>{isEdicao ? 'Editar Evento' : 'Novo Evento'}</h1>
        </div>

        <div style={ui.card}>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            <label style={ui.labelStyle}>
              Nome
              <input value={nome} onChange={(e) => setNome(e.target.value)} required style={ui.inputStyle} />
            </label>

            <label style={ui.labelStyle}>
              Local
              <input value={local} onChange={(e) => setLocal(e.target.value)} required style={ui.inputStyle} />
            </label>

            <label style={ui.labelStyle}>
              Data e Hora
              <input
                type="datetime-local"
                value={dataHora}
                onChange={(e) => setDataHora(e.target.value)}
                required
                style={ui.inputStyle}
              />
            </label>

            <label style={ui.labelStyle}>
              Capacidade total
              <input
                type="number"
                min={1}
                value={capacidadeTotal}
                onChange={(e) => setCapacidadeTotal(Number(e.target.value))}
                required
                style={ui.inputStyle}
              />
            </label>

            {erro && <p style={{ color: '#e94560', fontSize: 13, margin: 0 }}>{erro}</p>}

            <div style={{ display: 'flex', gap: 10, marginTop: 4 }}>
              <button type="button" style={ui.btnSecondary} onClick={() => navigate('/admin/eventos')}>
                Cancelar
              </button>
              <button type="submit" disabled={isPending} style={{ ...ui.btnPrimary, flex: 1 }}>
                {isPending ? 'Salvando…' : isEdicao ? 'Salvar alterações' : 'Criar evento'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  )
}
