# ADR-004: Autenticação JWT com Refresh Token e Autenticação Serviço-a-Serviço

## Status
Aceito

## Contexto

A plataforma tem dois tipos de identidade distintos:

1. **Usuários humanos** (clientes e administradores) que fazem login via browser/frontend
2. **Serviços internos** (ex: Ingressos.Api chamando Eventos.Api) que precisam se autenticar sem interação humana

As necessidades de cada caso são diferentes: usuários precisam de sessões longas com renovação transparente; serviços precisam de tokens de curta duração gerenciados automaticamente.

As abordagens avaliadas foram:
- **Apenas JWT de curta duração**: simples, mas exige reautenticação frequente do usuário
- **JWT + Refresh Token**: maior UX, tokens de acesso curtos com renovação transparente via refresh token de vida longa
- **OAuth2/OpenID Connect com servidor externo** (Keycloak, Auth0): completo, mas adiciona dependência externa e complexidade desnecessária para um projeto de portfólio
- **API Key para serviços internos**: mais simples, mas menos segura e não aproveita a infraestrutura JWT já existente

## Decisão

Adotamos **JWT de curta duração (access token) combinado com Refresh Token** para usuários, e um mecanismo de **JWT de serviço-a-serviço** para comunicação interna.

**Para usuários:**
- Access token JWT com expiração curta (configurável via `JwtOptions`)
- Refresh token opaco armazenado em banco (`RefreshToken`), com rotação a cada uso
- Endpoint `/api/auth/refresh` para renovação transparente

**Para serviços internos:**
- `ServicoTokenProvider`: obtém e cacheia um token JWT especial via `POST /api/auth/servico-interno`, autenticado por senha compartilhada (`ServicoInterno__Senha`)
- `AuthTokenDelegatingHandler`: intercepta chamadas HTTP de saída e injeta o token automaticamente
- Roles distintas: `Papeis.Administrador` para ações privilegiadas, `Papeis.Usuario` para operações comuns, `Papeis.ServicoInterno` para comunicação entre serviços

## Consequências

**Fica mais fácil:**
- Revogação efetiva de sessão via invalidação do refresh token no banco
- Serviços internos obtêm tokens automaticamente sem configuração manual de API Keys
- Separação clara de papéis no mesmo sistema de autorização (RBAC via Claims)
- Rate limiting no endpoint de login (5 req/min por IP/usuário) protege contra força bruta

**Fica mais difícil:**
- Refresh tokens precisam de armazenamento persistente e limpeza periódica de tokens expirados
- A senha compartilhada para serviços internos (`ServicoInterno__Senha`) deve ser gerenciada como secret (via variável de ambiente, nunca no código)
- Sem blacklist de access tokens: um token comprometido é válido até expirar (mitigado pela curta duração)
- Escalabilidade horizontal do Auth.Api requer que o segredo JWT e a senha interna sejam idênticos em todas as réplicas (via configuração centralizada)
