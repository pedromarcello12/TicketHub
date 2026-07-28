import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef } from 'react'
import { useAuth } from '../contexts/AuthContext'

export interface PagamentoStatusAlteradoPayload {
  pagamentoId: string
  ingressoId: string
  valor: number
  status: string
}

/**
 * Conecta ao NotificacoesHub do Pagamento.Api via SignalR e escuta
 * mudanças de status de pagamento em tempo real.
 *
 * - Entra automaticamente no grupo do usuário autenticado (via JWT)
 * - Exibe um toast com o novo status
 * - Invalida as queries React Query de pagamentos e ingressos
 *   para forçar atualização automática da UI
 */
export function useNotificacoes() {
  const { token } = useAuth()
  const queryClient = useQueryClient()
  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    if (!token) return

    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/notificacoes', {
        // Em dev o Vite proxy encaminha /hubs/* para localhost:5123 (Pagamento.Api)
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('PagamentoStatusAlterado', (payload: PagamentoStatusAlteradoPayload) => {
      mostrarToast(payload)

      // Invalida todas as queries de pagamentos e ingressos para recarregar dados
      queryClient.invalidateQueries({ queryKey: ['pagamentos'] })
      queryClient.invalidateQueries({ queryKey: ['meus-pagamentos'] })
      queryClient.invalidateQueries({ queryKey: ['ingressos'] })
    })

    connection
      .start()
      .catch(err => console.error('[SignalR] Erro ao conectar:', err))

    connectionRef.current = connection

    return () => {
      connection.stop()
      connectionRef.current = null
    }
  }, [token, queryClient])
}

// ─── Toast simples (sem dependência extra) ────────────────────────────────────

function mostrarToast(payload: PagamentoStatusAlteradoPayload) {
  const emojis: Record<string, string> = {
    Aprovado: '✅',
    Recusado: '❌',
    Estornado: '↩️',
  }
  const emoji = emojis[payload.status] ?? 'ℹ️'
  const valor = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(payload.valor)

  const toast = document.createElement('div')
  toast.setAttribute('role', 'status')
  toast.style.cssText = `
    position: fixed; bottom: 24px; right: 24px; z-index: 9999;
    background: #1e293b; color: #f8fafc;
    padding: 14px 20px; border-radius: 10px;
    font-family: system-ui, sans-serif; font-size: 14px;
    box-shadow: 0 8px 24px rgba(0,0,0,.35);
    max-width: 320px; line-height: 1.5;
    animation: slideIn .25s ease;
  `
  toast.innerHTML = `
    <strong>${emoji} Pagamento ${payload.status}</strong><br>
    Valor: ${valor}
  `

  // Injetar keyframe uma vez
  if (!document.getElementById('toast-style')) {
    const style = document.createElement('style')
    style.id = 'toast-style'
    style.textContent = `@keyframes slideIn { from { opacity:0; transform:translateY(12px) } to { opacity:1; transform:translateY(0) } }`
    document.head.appendChild(style)
  }

  document.body.appendChild(toast)
  setTimeout(() => toast.remove(), 5000)
}
