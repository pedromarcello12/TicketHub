import { useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ingressosApi } from '../../api/ingressos'
import { eventosApi } from '../../api/eventos'
import { Layout } from '../../components/Layout'
import * as ui from '../../components/ui'

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

  const { data: eventos = [] } = useQuery({
    queryKey: ['admin-eventos'],
    queryFn: eventosApi.listar,
  })

  useQuery({
    queryKey: ['ingresso', id],
    queryFn: () => ingressosApi.obterPorId(id!),
    enabled: isEdicao,
    onSuccess: (i) => {
      if (carregado) return
      setEventoId(i.eventoId)
      setTipoIngresso(i.tipoIngresso)
      setPreco(i.preco)
      setCarregado(true)
    },
  } as Parameters<typeof useQuery>[0])

  const criar = useMutation({
    mutationFn: ingressosApi.criar,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-ingressos'] })
      navigate('/admin/ingressos')
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar ingresso.')
    },
  })

  const atualizar = useMutation({
    mutationFn: (data: { tipoIngresso: string; preco: number }) =>
      ingressosApi.atualizar(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-ingressos'] })
      navigate('/admin/ingressos')
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setErro(e?.response?.data?.mensagem ?? 'Erro ao salvar ingresso.')
    },
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setErro('')
    if (isEdicao) {
      atualizar.mutate({ tipoIngresso, preco })
    } else {
      criar.mutate({ eventoId, tipoIngresso, preco })
    }
  }

  const isPending = criar.isPending || atualizar.isPending

  return (
    <Layout>
      <div style={{ maxWidth: 480 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 28 }}>
          <button style={ui.btnSecondary} onClick={() => navigate('/admin/ingressos')}>
            ← Voltar
          </button>
          <h1 style={ui.h1}>{isEdicao ? 'Editar Ingresso' : 'Novo Ingresso'}</h1>
        </div>

        <div style={ui.card}>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {!isEdicao && (
              <label style={ui.labelStyle}>
                Evento
                <select value={eventoId} onChange={(e) => setEventoId(e.target.value)} required style={ui.inputStyle}>
                  <option value="">Selecione um evento</option>
                  {eventos.map((ev) => (
                    <option key={ev.id} value={ev.id}>{ev.nome} ({ev.status})</option>
                  ))}
                </select>
              </label>
            )}

            <label style={ui.labelStyle}>
              Tipo
              <select value={tipoIngresso} onChange={(e) => setTipoIngresso(e.target.value)} style={ui.inputStyle}>
                <option>Inteira</option>
                <option>Meia-entrada</option>
                <option>VIP</option>
                <option>Camarote</option>
              </select>
            </label>

            <label style={ui.labelStyle}>
              Preço (R$)
              <input
                type="number"
                min={0}
                step={0.01}
                value={preco}
                onChange={(e) => setPreco(Number(e.target.value))}
                required
                style={ui.inputStyle}
              />
            </label>

            {erro && <p style={{ color: '#e94560', fontSize: 13, margin: 0 }}>{erro}</p>}

            <div style={{ display: 'flex', gap: 10, marginTop: 4 }}>
              <button type="button" style={ui.btnSecondary} onClick={() => navigate('/admin/ingressos')}>
                Cancelar
              </button>
              <button type="submit" disabled={isPending} style={{ ...ui.btnPrimary, flex: 1 }}>
                {isPending ? 'Salvando…' : isEdicao ? 'Salvar alterações' : 'Criar ingresso'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Layout>
  )
}
