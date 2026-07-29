import { useState, type FormEvent, type CSSProperties } from 'react'
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
    <div style={pageStyle}>
      <div style={cardStyle}>
        <h1 style={{ margin: '0 0 8px', color: '#1a1a2e', fontSize: 26 }}>🎟 TicketHub</h1>
        <p style={{ color: '#666', margin: '0 0 28px', fontSize: 14 }}>Acesse sua conta</p>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <label style={labelStyle}>
            Usuário
            <input
              type="text"
              value={nomeUsuario}
              onChange={(e) => setNomeUsuario(e.target.value)}
              required
              placeholder="ex: admin"
              style={inputStyle}
            />
          </label>

          <label style={labelStyle}>
            Senha
            <input
              type="password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
              style={inputStyle}
            />
          </label>

          {erro && <p style={{ color: '#e94560', fontSize: 13, margin: 0 }}>{erro}</p>}

          <button type="submit" disabled={loading} style={btnPrimary}>
            {loading ? 'Entrando…' : 'Entrar'}
          </button>
        </form>

        <p style={{ textAlign: 'center', marginTop: 20, fontSize: 13, color: '#666' }}>
          Não tem conta?{' '}
          <Link to="/registrar" style={{ color: '#e94560', textDecoration: 'none' }}>
            Criar conta
          </Link>
        </p>
      </div>
    </div>
  )
}

const pageStyle: CSSProperties = {
  minHeight: '100vh',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  background: '#f0f2f5',
  fontFamily: 'system-ui, sans-serif',
}

const cardStyle: CSSProperties = {
  background: '#fff',
  borderRadius: 12,
  padding: '40px 36px',
  width: '100%',
  maxWidth: 380,
  boxShadow: '0 4px 24px rgba(0,0,0,.10)',
  textAlign: 'left',
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
  outline: 'none',
}

const btnPrimary: CSSProperties = {
  background: '#e94560',
  color: '#fff',
  border: 'none',
  padding: '11px',
  borderRadius: 6,
  fontSize: 15,
  fontWeight: 600,
  cursor: 'pointer',
  marginTop: 4,
}
