import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { AuthProvider } from './contexts/AuthContext'
import { PrivateRoute, AdminRoute } from './components/PrivateRoute'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { EventosPage } from './pages/EventosPage'
import { MeusIngressosPage } from './pages/MeusIngressosPage'
import { AdminPage } from './pages/AdminPage'

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
            </Route>

            {/* Rotas admin */}
            <Route element={<AdminRoute />}>
              <Route path="/admin" element={<AdminPage />} />
              <Route path="/admin/criar-evento" element={<AdminPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  )
}
