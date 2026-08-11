import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { usuariosApi } from '../../api/usuarios'
import { useAuth } from '../../contexts/AuthContext'
import { Layout } from '../../components/Layout'

const papelTagCls: Record<string, string> = {
  Administrador: 'tag tag-accent',
  Cliente: 'tag tag-accent-2',
  Servico: 'tag tag-neutral',
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

  return (
    <Layout>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--space-6)' }}>
        <h1 style={{ margin: 0 }}>Usuários</h1>
        <button className="btn btn-primary" onClick={() => navigate('/registrar')}>
          <i className="ph ph-plus" /> Novo usuário
        </button>
      </div>

      <div className="card elev-sm" style={{ padding: 0, overflow: 'hidden' }}>
        {isLoading && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Carregando…</p>}
        {!isLoading && usuarios.length === 0 && <p className="text-muted" style={{ padding: 'var(--space-4)' }}>Nenhum usuário cadastrado.</p>}

        {usuarios.length > 0 && (
          <div style={{ overflowX: 'auto' }}>
            <table className="table">
              <thead>
                <tr><th>Nome</th><th>Usuário</th><th>Perfil</th><th>Ações</th></tr>
              </thead>
              <tbody>
                {usuarios.map((u) => (
                  <tr key={u.id}>
                    <td style={{ fontWeight: 500 }}>{u.nome}</td>
                    <td className="text-muted">{u.nomeUsuario}</td>
                    <td><span className={papelTagCls[u.papel] ?? 'tag tag-neutral'}>{u.papel}</span></td>
                    <td>
                      {u.nomeUsuario !== usuario?.nomeUsuario && u.papel !== 'Servico' && (
                        <button
                          className="btn btn-danger"
                          style={{ fontSize: 12, padding: '4px 10px' }}
                          onClick={() => { if (confirm(`Excluir "${u.nome}"?`)) excluir.mutate(u.id) }}
                          disabled={excluir.isPending}
                        >
                          <i className="ph ph-trash" /> Excluir
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
