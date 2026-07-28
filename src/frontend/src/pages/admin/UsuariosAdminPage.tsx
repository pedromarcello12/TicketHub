import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { usuariosApi } from '../../api/usuarios'
import { useAuth } from '../../contexts/AuthContext'
import { Layout } from '../../components/Layout'
import * as ui from '../../components/ui'

const PAPEL_COLOR: Record<string, string> = {
  Administrador: '#7c3aed',
  Cliente: '#3b82f6',
  Servico: '#6b7280',
}

export function UsuariosAdminPage() {
  const navigate = useNavigate()
  const { usuario } = useAuth()
  const qc = useQueryClient()

  const { data: usuarios = [], isLoading } = useQuery({
    queryKey: ['admin-usuarios'],
    queryFn: usuariosApi.listar,
  })

  const excluir = useMutation({
    mutationFn: usuariosApi.excluir,
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-usuarios'] }),
  })

  function confirmarExcluir(id: string, nome: string) {
    if (confirm(`Excluir usuário "${nome}"?`)) excluir.mutate(id)
  }

  return (
    <Layout>
      <div style={ui.pageHeader}>
        <h1 style={ui.h1}>Usuários</h1>
        <button style={ui.btnPrimary} onClick={() => navigate('/registrar')}>
          + Novo usuário
        </button>
      </div>

      <div style={ui.card}>
        {isLoading && <p style={{ color: '#888' }}>Carregando…</p>}
        {!isLoading && usuarios.length === 0 && (
          <p style={{ color: '#888' }}>Nenhum usuário cadastrado.</p>
        )}

        {usuarios.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table style={ui.table}>
              <thead>
                <tr>
                  <th style={ui.th}>Nome</th>
                  <th style={ui.th}>Usuário</th>
                  <th style={ui.th}>Perfil</th>
                  <th style={ui.th}>Ações</th>
                </tr>
              </thead>
              <tbody>
                {usuarios.map((u) => (
                  <tr key={u.id}>
                    <td style={ui.td}>{u.nome}</td>
                    <td style={ui.td}>{u.nomeUsuario}</td>
                    <td style={ui.td}>
                      <span style={{
                        background: PAPEL_COLOR[u.papel] ?? '#6b7280',
                        color: '#fff',
                        padding: '2px 10px',
                        borderRadius: 20,
                        fontSize: 11,
                        fontWeight: 600,
                      }}>
                        {u.papel}
                      </span>
                    </td>
                    <td style={ui.td}>
                      {u.nomeUsuario !== usuario?.nomeUsuario && u.papel !== 'Servico' && (
                        <button
                          style={ui.btnDanger}
                          onClick={() => confirmarExcluir(u.id, u.nome)}
                          disabled={excluir.isPending}
                        >
                          Excluir
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </Layout>
  )
}
