// ── Auth ──────────────────────────────────────────────────────────

export interface LoginRequest {
  nomeUsuario: string
  senha: string
}

export interface RegistrarRequest {
  nomeUsuario: string
  nome: string
  senha: string
  papel: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  nome: string
  papel: string
  email: string       // NomeUsuario do usuário (usado como identificador)
  expiraEm: string
}

// ── Eventos ───────────────────────────────────────────────────────

export interface EventoResponse {
  id: string
  nome: string
  local: string
  dataHora: string
  capacidadeTotal: number
  status: string
}

export interface CriarEventoRequest {
  nome: string
  local: string
  dataHora: string
  capacidadeTotal: number
}

export interface AtualizarEventoRequest {
  nome: string
  local: string
  dataHora: string
  capacidadeTotal: number
}

// ── Ingressos ─────────────────────────────────────────────────────

export interface IngressoResponse {
  id: string
  eventoId: string
  tipoIngresso: string
  preco: number
  status: string
  reservadoAte: string | null
}

export interface CriarIngressoRequest {
  eventoId: string
  tipoIngresso: string
  preco: number
}

export interface AtualizarIngressoRequest {
  tipoIngresso: string
  preco: number
}

export interface UsuarioAdminResponse {
  id: string
  nomeUsuario: string
  nome: string
  papel: string
}

// ── Perfil ────────────────────────────────────────────────────────

export interface PerfilResponse {
  id: string
  nomeUsuario: string
  nome: string
  papel: string
  email: string
}

export interface AtualizarPerfilRequest {
  nome?: string
  senhaAtual?: string
  novaSenha?: string
}

// ── Pagamento ─────────────────────────────────────────────────────

export type MetodoPagamento = 'CartaoCredito' | 'Pix' | 'Boleto'

export interface CriarPagamentoRequest {
  ingressoId: string
  valor: number
  metodo: number          // 1=CartaoCredito, 2=Pix, 3=Boleto
  emailCliente: string
}

export interface PagamentoResponse {
  id: string
  ingressoId: string
  valor: number
  metodo: string
  status: string
  emailCliente: string
}
