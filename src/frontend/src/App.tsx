import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { AuthProvider } from './contexts/AuthContext'
import { PrivateRoute, AdminRoute } from './components/PrivateRoute'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { EventosPage } from './pages/EventosPage'
import { MeusIngressosPage } from './pages/MeusIngressosPage'
import { ExtratoPage } from './pages/ExtratoPage'
import { PerfilPage } from './pages/PerfilPage'
import { EventosAdminPage } from './pages/admin/EventosAdminPage'
import { EventoFormPage } from './pages/admin/EventoFormPage'
import { IngressosAdminPage } from './pages/admin/IngressosAdminPage'
import { IngressoFormPage } from './pages/admin/IngressoFormPage'
import { PagamentosAdminPage } from './pages/admin/PagamentosAdminPage'
import { UsuariosAdminPage } from './pages/admin/UsuariosAdminPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30_000,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            {/* Rotas públicas */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/registrar" element={<RegisterPage />} />

            {/* Rotas autenticadas */}
            <Route element={<PrivateRoute />}>
              <Route path="/" element={<EventosPage />} />
              <Route path="/meus-ingressos" element={<MeusIngressosPage />} />
              <Route path="/extrato" element={<ExtratoPage />} />
              <Route path="/perfil" element={<PerfilPage />} />
            </Route>

            {/* Rotas admin */}
            <Route element={<AdminRoute />}>
              <Route path="/admin" element={<Navigate to="/admin/eventos" replace />} />
              <Route path="/admin/eventos" element={<EventosAdminPage />} />
              <Route path="/admin/eventos/novo" element={<EventoFormPage />} />
              <Route path="/admin/eventos/:id" element={<EventoFormPage />} />
              <Route path="/admin/ingressos" element={<IngressosAdminPage />} />
              <Route path="/admin/ingressos/novo" element={<IngressoFormPage />} />
              <Route path="/admin/ingressos/:id" element={<IngressoFormPage />} />
              <Route path="/admin/pagamentos" element={<PagamentosAdminPage />} />
              <Route path="/admin/usuarios" element={<UsuariosAdminPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  )
}
