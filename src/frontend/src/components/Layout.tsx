import { Link, useNavigate } from 'react-router-dom'
import type { CSSProperties } from 'react'
import { useAuth } from '../contexts/AuthContext'

export function Layout({ children }: { children: React.ReactNode }) {
  const { usuario, isAdmin, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <div style={{ minHeight: '100vh', background: '#f5f5f5', fontFamily: 'system-ui, sans-serif' }}>
      <header style={{
        background: '#1a1a2e',
        color: '#fff',
        padding: '0 24px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        height: 56,
        boxShadow: '0 2px 8px rgba(0,0,0,.3)',
      }}>
        <Link to="/" style={{ color: '#e94560', fontWeight: 700, fontSize: 20, textDecoration: 'none' }}>
          🎟 TicketHub
        </Link>
        <nav style={{ display: 'flex', alignItems: 'center', gap: 20 }}>
          <Link to="/" style={navLink}>Eventos</Link>
          <Link to="/meus-ingressos" style={navLink}>Meus Ingressos</Link>
          {isAdmin && <Link to="/admin" style={navLink}>Admin</Link>}
          <span style={{ color: '#aaa', fontSize: 14 }}>{usuario?.nome}</span>
          <button onClick={handleLogout} style={btnSmall}>Sair</button>
        </nav>
      </header>
      <main style={{ maxWidth: 1100, margin: '0 auto', padding: '32px 24px' }}>
        {children}
      </main>
    </div>
  )
}

const navLink: CSSProperties = {
  color: '#ccc',
  textDecoration: 'none',
  fontSize: 14,
  fontWeight: 500,
}

const btnSmall: CSSProperties = {
  background: 'transparent',
  border: '1px solid #e94560',
  color: '#e94560',
  padding: '4px 12px',
  borderRadius: 4,
  cursor: 'pointer',
  fontSize: 13,
}
