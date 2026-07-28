import {
  createContext,
  useContext,
  useState,
  useCallback,
  type ReactNode,
} from 'react'
import type { LoginResponse } from '../types'

interface UsuarioInfo {
  nome: string
  papel: string
  nomeUsuario: string
}

interface AuthContextValue {
  token: string | null
  usuario: UsuarioInfo | null
  isAdmin: boolean
  login: (res: LoginResponse) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function carregarUsuario(): UsuarioInfo | null {
  try {
    const raw = localStorage.getItem('usuario')
    return raw ? (JSON.parse(raw) as UsuarioInfo) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'))
  const [usuario, setUsuario] = useState<UsuarioInfo | null>(carregarUsuario)

  const login = useCallback((res: LoginResponse) => {
    localStorage.setItem('token', res.token)
    const info: UsuarioInfo = { nome: res.nome, papel: res.papel, nomeUsuario: res.email }
    localStorage.setItem('usuario', JSON.stringify(info))
    setToken(res.token)
    setUsuario(info)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
    setToken(null)
    setUsuario(null)
  }, [])

  const isAdmin = usuario?.papel === 'Administrador'

  return (
    <AuthContext.Provider value={{ token, usuario, isAdmin, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider')
  return ctx
}
