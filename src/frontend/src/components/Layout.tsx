import { Link, NavLink, useNavigate } from 'react-router-dom'
import type { CSSProperties } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNotificacoes } from '../hooks/useNotificacoes'

export function Layout({ children }: { children: React.ReactNode }) {
  const { usuario, isAdmin, logout } = useAuth()
  useNotificacoes() // conecta ao SignalR enquanto o usuário estiver autenticado
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
          <NavLink to="/" end style={navLinkStyle}>Eventos</NavLink>
          <NavLink to="/meus-ingressos" style={navLinkStyle}>Meus Ingressos</NavLink>
          <NavLink to="/extrato" style={navLinkStyle}>Extrato</NavLink>
          {isAdmin && (
            <>
              <span style={{ color: '#444', fontSize: 14 }}>|</span>
              <NavLink to="/admin/eventos" style={navLinkStyle}>Admin: Eventos</NavLink>
              <NavLink to="/admin/ingressos" style={navLinkStyle}>Ingressos</NavLink>
              <NavLink to="/admin/pagamentos" style={navLinkStyle}>Pagamentos</NavLink>
              <NavLink to="/admin/usuarios" style={navLinkStyle}>Usuários</NavLink>
            </>
          )}
          <NavLink to="/perfil" style={navLinkStyle}>{usuario?.nome}</NavLink>
          <button onClick={handleLogout} style={btnSmall}>Sair</button>
        </nav>
      </header>
      <main style={{ maxWidth: 1100, margin: '0 auto', padding: '32px 24px' }}>
        {children}
      </main>
    </div>
  )
}

function navLinkStyle({ isActive }: { isActive: boolean }): CSSProperties {
  return {
    color: isActive ? '#e94560' : '#ccc',
    textDecoration: 'none',
    fontSize: 14,
    fontWeight: isActive ? 700 : 500,
  }
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
