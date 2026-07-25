# ADR-003: Database-per-Service em vez de Banco Compartilhado

## Status
Aceito

## Contexto

Em uma arquitetura de microsserviços, a estratégia de persistência define o grau real de independência entre os serviços. As opções avaliadas foram:

1. **Banco compartilhado**: todos os serviços acessam o mesmo banco SQL Server, potencialmente com schemas separados por serviço.
2. **Database-per-service**: cada serviço tem seu próprio banco de dados lógico, inacessível diretamente pelos demais.

## Decisão

Adotamos **database-per-service**. Cada serviço tem seu próprio banco SQL Server:

- `TicketHubAuth` — gerenciado por `AuthDbContext`
- `TicketHubEventos` — gerenciado por `EventosDbContext`
- `TicketHubIngressos` — gerenciado por `IngressosDbContext`
- `TicketHubPagamentos` — gerenciado por `PagamentosDbContext`

A comunicação entre serviços é feita exclusivamente por suas APIs HTTP públicas (autenticadas via JWT de serviço-a-serviço) ou por eventos de integração no RabbitMQ — nunca por acesso direto ao banco alheio.

Cada serviço gerencia suas próprias migrations (`EF Core Migrations`) e pode evoluir seu schema de forma independente.

## Consequências

**Fica mais fácil:**
- Deploy e schema evolution independentes: migrar o Pagamento.Api não afeta os demais serviços
- Falha no banco de um serviço não derruba os demais
- Cada serviço pode escolher a tecnologia de persistência mais adequada ao seu domínio no futuro (ex: EventStore para eventos de domínio)
- Fronteiras de domínio claras: não há risco de um serviço acessar dados de outro diretamente

**Fica mais difícil:**
- Joins entre entidades de serviços diferentes são impossíveis a nível de banco — devem ser feitos na camada de aplicação via chamadas HTTP ou composição de eventos
- Consistência eventual: confirmar uma venda exige coordenação entre Ingressos e Pagamento sem transaction ACID distribuída
- Operações locais em desenvolvimento requerem 4 connection strings distintas e 4 bancos inicializados (mitigado pelo Docker Compose com healthchecks)
- Relatórios que cruzam dados de múltiplos serviços exigem uma camada de query separada (ex: read model/projeção, ou um serviço de relatórios dedicado)
