import { useState, type FormEvent } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { useAuth } from '../contexts/AuthContext'
import { Layout } from '../components/Layout'
import * as ui from '../components/ui'

const PAPEL_COLOR: Record<string, string> = {
  Administrador: '#7c3aed',
  Cliente: '#3b82f6',
  Servico: '#6b7280',
}

export function PerfilPage() {
  const { login } = useAuth()
  const qc = useQueryClient()

  const { data: perfil, isLoading } = useQuery({
    queryKey: ['perfil'],
    queryFn: authApi.obterPerfil,
  })

  // ─ Atualizar nome ─
  const [nome, setNome] = useState('')
  const [nomeErro, setNomeErro] = useState('')
  const [nomeSucesso, setNomeSucesso] = useState(false)

  // ─ Alterar senha ─
  const [senhaAtual, setSenhaAtual] = useState('')
  const [novaSenha, setNovaSenha] = useState('')
  const [confirmar, setConfirmar] = useState('')
  const [senhaErro, setSenhaErro] = useState('')
  const [senhaSucesso, setSenhaSucesso] = useState(false)

  const atualizarNome = useMutation({
    mutationFn: () => authApi.atualizarPerfil({ nome }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['perfil'] })
      setNomeSucesso(true)
      setNomeErro('')
      setTimeout(() => setNomeSucesso(false), 3000)
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setNomeErro(e?.response?.data?.mensagem ?? 'Erro ao atualizar nome.')
      setNomeSucesso(false)
    },
  })

  const alterarSenha = useMutation({
    mutationFn: () => authApi.atualizarPerfil({ senhaAtual, novaSenha }),
    onSuccess: () => {
      setSenhaSucesso(true)
      setSenhaErro('')
      setSenhaAtual('')
      setNovaSenha('')
      setConfirmar('')
      setTimeout(() => setSenhaSucesso(false), 3000)
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setSenhaErro(e?.response?.data?.mensagem ?? 'Erro ao alterar senha.')
      setSenhaSucesso(false)
    },
  })

  function handleNome(e: FormEvent) {
    e.preventDefault()
    setNomeErro('')
    if (!nome.trim()) { setNomeErro('O nome não pode ser vazio.'); return }
    atualizarNome.mutate()
  }

  function handleSenha(e: FormEvent) {
    e.preventDefault()
    setSenhaErro('')
    if (novaSenha !== confirmar) { setSenhaErro('As senhas não conferem.'); return }
    if (novaSenha.length < 6) { setSenhaErro('A nova senha deve ter ao menos 6 caracteres.'); return }
    alterarSenha.mutate()
  }

  return (
    <Layout>
      <h1 style={{ ...ui.h1, marginBottom: 28 }}>Meu Perfil</h1>

      {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}

      {perfil && (
        <div style={{ maxWidth: 480, display: 'flex', flexDirection: 'column', gap: 24 }}>

          {/* Dados atuais */}
          <div style={ui.card}>
            <h2 style={{ margin: '0 0 16px', fontSize: 16, color: '#1a1a2e' }}>Dados da conta</h2>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              <InfoRow label="Usuário" value={perfil.nomeUsuario} />
              <InfoRow label="Nome" value={perfil.nome} />
              <InfoRow label="Perfil" value={
                <span style={{
                  background: PAPEL_COLOR[perfil.papel] ?? '#6b7280',
                  color: '#fff',
                  padding: '2px 10px',
                  borderRadius: 20,
                  fontSize: 12,
                  fontWeight: 600,
                }}>
                  {perfil.papel}
                </span>
              } />
            </div>
          </div>

          {/* Atualizar nome */}
          <div style={ui.card}>
            <h2 style={{ margin: '0 0 16px', fontSize: 16, color: '#1a1a2e' }}>Atualizar nome</h2>
            <form onSubmit={handleNome} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label style={ui.labelStyle}>
                Novo nome
                <input
                  value={nome}
                  onChange={(e) => setNome(e.target.value)}
                  placeholder={perfil.nome}
                  style={ui.inputStyle}
                />
              </label>
              {nomeErro && <p style={{ color: '#e94560', fontSize: 13, margin: 0 }}>{nomeErro}</p>}
              {nomeSucesso && <p style={{ color: '#16a34a', fontSize: 13, margin: 0 }}>Nome atualizado!</p>}
              <button type="submit" style={ui.btnPrimary} disabled={atualizarNome.isPending}>
                {atualizarNome.isPending ? 'Salvando…' : 'Salvar nome'}
              </button>
            </form>
          </div>

          {/* Alterar senha */}
          <div style={ui.card}>
            <h2 style={{ margin: '0 0 16px', fontSize: 16, color: '#1a1a2e' }}>Alterar senha</h2>
            <form onSubmit={handleSenha} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <label style={ui.labelStyle}>
                Senha atual
                <input
                  type="password"
                  value={senhaAtual}
                  onChange={(e) => setSenhaAtual(e.target.value)}
                  required
                  style={ui.inputStyle}
                />
              </label>
              <label style={ui.labelStyle}>
                Nova senha
                <input
                  type="password"
                  value={novaSenha}
                  onChange={(e) => setNovaSenha(e.target.value)}
                  required
                  style={ui.inputStyle}
                />
              </label>
              <label style={ui.labelStyle}>
                Confirmar nova senha
                <input
                  type="password"
                  value={confirmar}
                  onChange={(e) => setConfirmar(e.target.value)}
                  required
                  style={ui.inputStyle}
                />
              </label>
              {senhaErro && <p style={{ color: '#e94560', fontSize: 13, margin: 0 }}>{senhaErro}</p>}
              {senhaSucesso && <p style={{ color: '#16a34a', fontSize: 13, margin: 0 }}>Senha alterada com sucesso!</p>}
              <button type="submit" style={ui.btnPrimary} disabled={alterarSenha.isPending}>
                {alterarSenha.isPending ? 'Alterando…' : 'Alterar senha'}
              </button>
            </form>
          </div>

        </div>
      )}
    </Layout>
  )
}

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
      <span style={{ fontSize: 13, color: '#888', width: 72, flexShrink: 0 }}>{label}</span>
      <span style={{ fontSize: 14, color: '#1a1a2e', fontWeight: 500 }}>{value}</span>
    </div>
  )
}
