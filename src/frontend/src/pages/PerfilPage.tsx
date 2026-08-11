import { useState, type FormEvent } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api/auth'
import { Layout } from '../components/Layout'

const papelTagCls: Record<string, string> = {
  Administrador: 'tag tag-accent',
  Cliente: 'tag tag-accent-2',
  Servico: 'tag tag-neutral',
}

export function PerfilPage() {
  const qc = useQueryClient()

  const { data: perfil, isLoading } = useQuery({
    queryKey: ['perfil'],
    queryFn: authApi.obterPerfil,
  })

  const [nome, setNome] = useState('')
  const [nomeErro, setNomeErro] = useState('')
  const [nomeSucesso, setNomeSucesso] = useState(false)

  const [senhaAtual, setSenhaAtual] = useState('')
  const [novaSenha, setNovaSenha] = useState('')
  const [confirmar, setConfirmar] = useState('')
  const [senhaErro, setSenhaErro] = useState('')
  const [senhaSucesso, setSenhaSucesso] = useState(false)

  const atualizarNome = useMutation({
    mutationFn: () => authApi.atualizarPerfil({ nome }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['perfil'] })
      setNomeSucesso(true); setNomeErro('')
      setTimeout(() => setNomeSucesso(false), 3000)
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setNomeErro(e?.response?.data?.mensagem ?? 'Erro ao atualizar nome.')
    },
  })

  const alterarSenha = useMutation({
    mutationFn: () => authApi.atualizarPerfil({ senhaAtual, novaSenha }),
    onSuccess: () => {
      setSenhaSucesso(true); setSenhaErro('')
      setSenhaAtual(''); setNovaSenha(''); setConfirmar('')
      setTimeout(() => setSenhaSucesso(false), 3000)
    },
    onError: (err: unknown) => {
      const e = err as { response?: { data?: { mensagem?: string } } }
      setSenhaErro(e?.response?.data?.mensagem ?? 'Erro ao alterar senha.')
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
      <h1 style={{ marginBottom: 'var(--space-6)' }}>Meu perfil</h1>

      {isLoading && <p className="text-muted">Carregando…</p>}

      {perfil && (
        <div style={{ maxWidth: 480, display: 'flex', flexDirection: 'column', gap: 20 }}>

          {/* Dados da conta */}
          <div className="card elev-sm" style={{ gap: 16 }}>
            <div className="card-kicker">Dados da conta</div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              <InfoRow label="Usuário" value={perfil.nomeUsuario} />
              <InfoRow label="Nome" value={perfil.nome} />
              <InfoRow label="Perfil" value={
                <span className={papelTagCls[perfil.papel] ?? 'tag tag-neutral'}>{perfil.papel}</span>
              } />
            </div>
          </div>

          {/* Atualizar nome */}
          <div className="card elev-sm" style={{ gap: 16 }}>
            <div className="card-kicker">Atualizar nome</div>
            <form onSubmit={handleNome} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div className="field">
                <label>Novo nome</label>
                <input
                  className="input"
                  value={nome}
                  onChange={(e) => setNome(e.target.value)}
                  placeholder={perfil.nome}
                />
              </div>
              {nomeErro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{nomeErro}</p>}
              {nomeSucesso && <p style={{ color: 'var(--color-success)', fontSize: 13, margin: 0 }}>Nome atualizado!</p>}
              <button type="submit" className="btn btn-primary" disabled={atualizarNome.isPending}>
                {atualizarNome.isPending ? 'Salvando…' : 'Salvar nome'}
              </button>
            </form>
          </div>

          {/* Alterar senha */}
          <div className="card elev-sm" style={{ gap: 16 }}>
            <div className="card-kicker">Alterar senha</div>
            <form onSubmit={handleSenha} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div className="field">
                <label>Senha atual</label>
                <input className="input" type="password" value={senhaAtual} onChange={(e) => setSenhaAtual(e.target.value)} required />
              </div>
              <div className="field">
                <label>Nova senha</label>
                <input className="input" type="password" value={novaSenha} onChange={(e) => setNovaSenha(e.target.value)} required />
              </div>
              <div className="field">
                <label>Confirmar nova senha</label>
                <input className="input" type="password" value={confirmar} onChange={(e) => setConfirmar(e.target.value)} required />
              </div>
              {senhaErro && <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{senhaErro}</p>}
              {senhaSucesso && <p style={{ color: 'var(--color-success)', fontSize: 13, margin: 0 }}>Senha alterada com sucesso!</p>}
              <button type="submit" className="btn btn-primary" disabled={alterarSenha.isPending}>
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
    <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
      <span className="text-muted" style={{ fontSize: 12, width: 72, flexShrink: 0 }}>{label}</span>
      <span style={{ fontSize: 14 }}>{value}</span>
    </div>
  )
}
