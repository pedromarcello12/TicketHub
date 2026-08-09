import { useState, type FormEvent } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { authApi } from '../api/auth'
import { useAuth } from '../contexts/AuthContext'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [nomeUsuario, setNomeUsuario] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setErro('')
    setLoading(true)
    try {
      const res = await authApi.login({ nomeUsuario, senha })
      login(res)
      navigate('/')
    } catch {
      setErro('Usuário ou senha inválidos.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{
      minHeight: '100vh', background: 'var(--color-bg)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      padding: 'var(--space-4)',
    }}>
      <div style={{ width: '100%', maxWidth: 380 }}>
        {/* Brand */}
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <div style={{ display: 'inline-flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
            <i className="ph-fill ph-ticket" style={{ fontSize: 28, color: 'var(--color-accent)' }} />
            <span style={{ fontSize: 24, fontFamily: 'var(--font-heading)', fontWeight: 500 }}>TicketHub</span>
          </div>
          <p className="text-muted" style={{ margin: 0, fontSize: 14 }}>Acesse sua conta</p>
        </div>

        <div className="card elev-md" style={{ padding: 28, gap: 20 }}>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            <div className="field">
              <label>Usuário</label>
              <input
                className="input"
                type="text"
                value={nomeUsuario}
                onChange={(e) => setNomeUsuario(e.target.value)}
                required
                placeholder="ex: admin"
                autoFocus
              />
            </div>

            <div className="field">
              <label>Senha</label>
              <input
                className="input"
                type="password"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                required
              />
            </div>

            {erro && (
              <p style={{ color: 'var(--color-danger)', fontSize: 13, margin: 0 }}>{erro}</p>
            )}

            <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
              {loading ? 'Entrando…' : 'Entrar'}
            </button>
          </form>

          <div className="hr" style={{ margin: '4px 0' }} />

          <p style={{ textAlign: 'center', margin: 0, fontSize: 13 }} className="text-muted">
            Não tem conta?{' '}
            <Link to="/registrar" style={{ color: 'var(--color-accent)', textDecoration: 'none' }}>
              Criar conta
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}
