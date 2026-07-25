# TicketHub — Arquitetura C4

Documentação arquitetural em três níveis do modelo C4 (Context → Container → Component).

---

## Nível 1 — Context

Visão de alto nível: quem usa o sistema e com quais sistemas externos ele interage.

```mermaid
C4Context
    title TicketHub — Diagrama de Contexto

    Person(cliente, "Cliente", "Usuário que navega por eventos, reserva ingressos e efetua pagamentos.")
    Person(admin, "Administrador", "Gerencia eventos, aprova/recusa pagamentos.")

    System(tickethub, "TicketHub", "Plataforma distribuída de venda de ingressos. Permite reserva com janela de 10 minutos, pagamento e envio de QR Code por e-mail.")

    System_Ext(smtp, "Servidor SMTP", "Responsável pelo envio de e-mails de notificação (MailHog em desenvolvimento, SMTP externo em produção).")

    Rel(cliente, tickethub, "Navega por eventos, reserva ingressos, efetua pagamentos", "HTTPS / REST")
    Rel(admin, tickethub, "Cadastra eventos, publica/cancela eventos, aprova/recusa pagamentos", "HTTPS / REST")
    Rel(tickethub, smtp, "Envia notificações de status de pagamento", "SMTP")
```

---

## Nível 2 — Container

Detalha os processos e bancos de dados que compõem o TicketHub.

```mermaid
C4Container
    title TicketHub — Diagrama de Containers

    Person(cliente, "Cliente")
    Person(admin, "Administrador")
    System_Ext(smtp, "SMTP")

    System_Boundary(tickethub, "TicketHub") {

        Container(auth_api, "Auth.Api", ".NET 10 / ASP.NET Core", "Autenticação e autorização. Emite JWT, gerencia refresh tokens e valida credenciais de serviços internos.")
        ContainerDb(auth_db, "Auth DB", "SQL Server", "Usuários e refresh tokens.")

        Container(eventos_api, "Eventos.Api", ".NET 10 / ASP.NET Core", "CRUD de eventos com máquina de estados (Rascunho → Publicado → Encerrado/Cancelado).")
        ContainerDb(eventos_db, "Eventos DB", "SQL Server", "Eventos e seus metadados.")

        Container(ingressos_api, "Ingressos.Api", ".NET 10 / ASP.NET Core", "Reserva e ciclo de vida de ingressos. Aplica TTL de 10 minutos via background worker.")
        ContainerDb(ingressos_db, "Ingressos DB", "SQL Server", "Ingressos e estado de reserva.")

        Container(pagamento_api, "Pagamento.Api", ".NET 10 / ASP.NET Core", "Processamento de pagamentos com resiliência via Polly. Publica eventos no RabbitMQ após aprovação/recusa.")
        ContainerDb(pagamento_db, "Pagamentos DB", "SQL Server", "Registros de pagamentos.")

        Container(notificacoes_worker, "Notificacoes.Worker", ".NET 10 / Worker Service", "Consome eventos do RabbitMQ e envia e-mail de notificação ao cliente.")

        ContainerDb(rabbitmq, "RabbitMQ", "RabbitMQ 3", "Message broker para eventos de integração assíncronos entre serviços.")
        ContainerDb(redis, "Redis", "Redis 7", "Cache distribuído. Disponível para rate limiting e cache de dados quentes.")
        ContainerDb(seq, "Seq", "Seq", "Agregador de logs estruturados. Recebe eventos Serilog de todos os serviços via HTTP.")
    }

    Rel(cliente, auth_api, "POST /api/auth/login, /refresh, /registrar", "HTTPS")
    Rel(admin, auth_api, "POST /api/auth/login", "HTTPS")

    Rel(cliente, eventos_api, "GET /api/eventos", "HTTPS / JWT")
    Rel(admin, eventos_api, "POST /api/eventos, /publicar, /cancelar, /encerrar", "HTTPS / JWT")

    Rel(cliente, ingressos_api, "GET, POST /api/ingressos, /reservar", "HTTPS / JWT")
    Rel(admin, ingressos_api, "POST /api/ingressos (criar), /confirmar-venda, /cancelar", "HTTPS / JWT")

    Rel(cliente, pagamento_api, "POST /api/pagamentos", "HTTPS / JWT")
    Rel(admin, pagamento_api, "POST /api/pagamentos/:id/aprovar, /recusar, /estornar", "HTTPS / JWT")

    Rel(auth_api, auth_db, "Lê/Escreve usuários e refresh tokens", "EF Core")
    Rel(eventos_api, eventos_db, "Lê/Escreve eventos", "EF Core")
    Rel(ingressos_api, ingressos_db, "Lê/Escreve ingressos", "EF Core")
    Rel(pagamento_api, pagamento_db, "Lê/Escreve pagamentos", "EF Core")

    Rel(ingressos_api, eventos_api, "Valida existência do evento ao criar ingresso", "HTTP / JWT Serviço Interno + Polly")
    Rel(pagamento_api, ingressos_api, "Valida existência do ingresso ao criar pagamento", "HTTP / JWT Serviço Interno + Polly")
    Rel(ingressos_api, auth_api, "Obtém token de serviço interno", "HTTP")
    Rel(pagamento_api, auth_api, "Obtém token de serviço interno", "HTTP")

    Rel(pagamento_api, rabbitmq, "Publica PagamentoStatusAlteradoEvent", "AMQP")
    Rel(notificacoes_worker, rabbitmq, "Consome PagamentoStatusAlteradoEvent", "AMQP")
    Rel(notificacoes_worker, smtp, "Envia e-mail de notificação", "SMTP")

    Rel(ingressos_api, redis, "Cache distribuído", "TCP")

    Rel(auth_api, seq, "Logs estruturados", "HTTP / Serilog")
    Rel(eventos_api, seq, "Logs estruturados", "HTTP / Serilog")
    Rel(ingressos_api, seq, "Logs estruturados", "HTTP / Serilog")
    Rel(pagamento_api, seq, "Logs estruturados", "HTTP / Serilog")
    Rel(notificacoes_worker, seq, "Logs estruturados", "HTTP / Serilog")
```

---

## Nível 3 — Component (Ingressos.Api)

Detalhamento interno do serviço mais complexo do sistema, que aplica DDD e concentra as regras de negócio de reserva.

```mermaid
C4Component
    title Ingressos.Api — Diagrama de Componentes

    Container_Ext(cliente_http, "Cliente HTTP", "Frontend ou outro serviço")
    Container_Ext(eventos_api, "Eventos.Api", "Serviço externo")
    Container_Ext(ingressos_db, "Ingressos DB", "SQL Server")
    Container_Ext(auth_api, "Auth.Api", "Serviço externo")

    Container_Boundary(ingressos, "Ingressos.Api") {

        Component(controller, "IngressosController", "ASP.NET Core Controller", "Recebe requisições HTTP, valida autenticação/autorização via JWT e delega ao Application Service.")

        Component(app_service, "IngressoAppService", "Application Service", "Orquestra o fluxo de negócio: cria, reserva, confirma venda e cancela ingressos. Coordena repositório e serviços externos.")

        Component(dominio, "Ingresso (Agregado)", "Domain Entity", "Encapsula todas as regras de negócio: invariantes de estado, TTL de 10 minutos, transições válidas (Disponível → Reservado → Vendido/Cancelado).")

        Component(repositorio, "IngressoRepositorio", "EF Core Repository", "Persiste e recupera agregados Ingresso no SQL Server.")

        Component(http_evento_service, "HttpEventoExternalService", "HTTP Client + Polly", "Chama Eventos.Api para validar existência do evento. Aplica retry exponencial e circuit breaker via Polly.")

        Component(auth_handler, "AuthTokenDelegatingHandler", "HTTP Message Handler", "Injeta automaticamente o token JWT de serviço interno em todas as chamadas HTTP de saída.")

        Component(token_provider, "ServicoTokenProvider", "Singleton Cache", "Obtém e cacheia o token JWT de serviço interno via Auth.Api.")

        Component(expiracao_worker, "LiberacaoReservaExpiradaWorker", "IHostedService", "Background worker que executa periodicamente, busca reservas com TTL expirado e chama LiberarReservaExpirada() em cada agregado.")
    }

    Rel(cliente_http, controller, "HTTP / JWT", "REST")
    Rel(controller, app_service, "Delega operações")
    Rel(app_service, dominio, "Aplica regras de negócio")
    Rel(app_service, repositorio, "Lê e persiste agregados")
    Rel(app_service, http_evento_service, "Valida evento ao criar ingresso")
    Rel(http_evento_service, auth_handler, "Adiciona JWT ao request")
    Rel(auth_handler, token_provider, "Obtém token cacheado")
    Rel(token_provider, auth_api, "POST /api/auth/servico-interno", "HTTP")
    Rel(http_evento_service, eventos_api, "GET /api/eventos/:id", "HTTP + Polly")
    Rel(repositorio, ingressos_db, "EF Core / SQL Server")
    Rel(expiracao_worker, repositorio, "Busca reservas expiradas e salva alterações")
    Rel(expiracao_worker, dominio, "Chama LiberarReservaExpirada()")
```

---

## Nível 3 — Component (Pagamento.Api)

```mermaid
C4Component
    title Pagamento.Api — Diagrama de Componentes

    Container_Ext(cliente_http, "Cliente HTTP", "Frontend ou outro serviço")
    Container_Ext(ingressos_api, "Ingressos.Api", "Serviço externo")
    Container_Ext(pagamento_db, "Pagamentos DB", "SQL Server")
    Container_Ext(rabbitmq, "RabbitMQ", "Message Broker")
    Container_Ext(auth_api, "Auth.Api", "Serviço externo")

    Container_Boundary(pagamento, "Pagamento.Api") {

        Component(controller, "PagamentosController", "ASP.NET Core Controller", "Expõe endpoints para criar pagamentos e realizar transições de status (aprovar, recusar, estornar).")

        Component(app_service, "PagamentoAppService", "Application Service", "Orquestra o fluxo: valida ingresso, cria pagamento, aplica transições de estado e publica eventos de integração.")

        Component(dominio, "Pagamento (Entidade)", "Domain Entity", "Encapsula regras de negócio de pagamento: transições válidas de status (Pendente → Aprovado/Recusado → Estornado).")

        Component(repositorio, "PagamentoRepositorio", "EF Core Repository", "Persiste e recupera entidades Pagamento no SQL Server.")

        Component(http_ingresso_service, "HttpIngressoExternalService", "HTTP Client + Polly", "Valida existência do ingresso no Ingressos.Api antes de criar um pagamento.")

        Component(evento_publisher, "PagamentoEventoPublisher", "RabbitMQ Publisher", "Publica PagamentoStatusAlteradoEvent no exchange do RabbitMQ após cada transição de status.")

        Component(auth_handler, "AuthTokenDelegatingHandler", "HTTP Message Handler", "Injeta automaticamente o JWT de serviço interno nas chamadas HTTP de saída.")

        Component(token_provider, "ServicoTokenProvider", "Singleton Cache", "Obtém e cacheia o token JWT de serviço interno via Auth.Api.")
    }

    Rel(cliente_http, controller, "HTTP / JWT", "REST")
    Rel(controller, app_service, "Delega operações")
    Rel(app_service, dominio, "Aplica regras de negócio")
    Rel(app_service, repositorio, "Lê e persiste entidades")
    Rel(app_service, http_ingresso_service, "Valida ingresso ao criar pagamento")
    Rel(app_service, evento_publisher, "Publica evento após transição de status")
    Rel(http_ingresso_service, auth_handler, "Adiciona JWT ao request")
    Rel(auth_handler, token_provider, "Obtém token cacheado")
    Rel(token_provider, auth_api, "POST /api/auth/servico-interno", "HTTP")
    Rel(http_ingresso_service, ingressos_api, "GET /api/ingressos/:id", "HTTP + Polly")
    Rel(repositorio, pagamento_db, "EF Core / SQL Server")
    Rel(evento_publisher, rabbitmq, "AMQP / Exchange tickethub.eventos")
```
