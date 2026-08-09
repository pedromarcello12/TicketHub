import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useNotificacoes } from '../hooks/useNotificacoes'

export function Layout({ children }: { children: React.ReactNode }) {
  const { usuario, isAdmin, logout } = useAuth()
  useNotificacoes()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <div style={{ minHeight: '100vh', background: 'var(--color-bg)' }}>
      <header className="nav">
        <NavLink to="/" className="nav-brand">
          <i className="ph-fill ph-ticket" style={{ fontSize: 20, color: 'var(--color-accent)' }} />
          TicketHub
        </NavLink>

        <nav style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)' }}>
          <NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
            Eventos
          </NavLink>
          <NavLink to="/meus-ingressos" className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
            Meus ingressos
          </NavLink>
          <NavLink to="/extrato" className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
            Extrato
          </NavLink>

          {isAdmin && (
            <>
              <span style={{ width: 1, height: 16, background: 'var(--color-divider)', margin: '0 4px' }} />
              <NavLink to="/admin/eventos" className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
                Eventos
              </NavLink>
              <NavLink to="/admin/ingressos" className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
                Ingressos
              </NavLink>
              <NavLink to="/admin/pagamentos" className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
                Pagamentos
              </NavLink>
              <NavLink to="/admin/usuarios" className={({ isActive }) => isActive ? 'active' : ''} style={navLink}>
                Usuários
              </NavLink>
            </>
          )}
        </nav>

        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
          <NavLink to="/perfil" style={{ ...navLink, textDecoration: 'none' }}>
            <button className="btn btn-secondary" style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <i className="ph ph-user" style={{ fontSize: 15 }} />
              {usuario?.nome}
            </button>
          </NavLink>
          <button className="btn btn-ghost" onClick={handleLogout}>
            <i className="ph ph-sign-out" style={{ fontSize: 15 }} />
            Sair
          </button>
        </div>
      </header>

      <main style={{ maxWidth: 1120, margin: '0 auto', padding: '32px var(--space-6) 64px' }}>
        {children}
      </main>
    </div>
  )
}

const navLink = { color: 'inherit', textDecoration: 'none', fontSize: 14 }
