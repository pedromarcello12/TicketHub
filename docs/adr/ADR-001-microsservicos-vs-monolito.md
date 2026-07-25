# ADR-001: Arquitetura de Microsserviços em vez de Monolito Modular

## Status
Aceito

## Contexto

O TicketHub é uma plataforma de venda de ingressos com domínios bem delimitados: cadastro de eventos, reserva e emissão de ingressos, processamento de pagamento e envio de notificações. A escolha da arquitetura base determina como esses domínios se relacionam, escalam e evoluem.

As duas abordagens principais consideradas foram:

- **Monolito Modular**: um único processo com módulos internos fortemente delimitados por namespaces e interfaces, compartilhando o mesmo banco de dados e processo de deploy.
- **Microsserviços**: serviços independentes, cada um com seu próprio banco de dados, processo de build e deploy.

O projeto tem natureza de portfólio, com foco explícito em demonstrar escalabilidade, resiliência e boas práticas de design distribuído (DDD, CQRS, database-per-service).

## Decisão

Adotamos a **arquitetura de microsserviços**, dividindo o sistema em quatro serviços independentes:

- **Eventos.Api** — cadastro e consulta de eventos
- **Ingressos.Api** — reserva, emissão e ciclo de vida de ingressos
- **Pagamento.Api** — processamento e transições de status de pagamentos
- **Notificacoes.Worker** — consumo de eventos do message broker e envio de e-mails

Cada serviço tem seu próprio banco de dados SQL Server e se comunica com os demais via HTTP (chamadas síncronas entre serviços para validação) e RabbitMQ (eventos de integração assíncronos).

## Consequências

**Fica mais fácil:**
- Escalar serviços individualmente (ex: Ingressos.Api sob alta demanda em lançamentos)
- Fazer deploy independente de cada serviço sem afetar os demais
- Aplicar tecnologias diferentes por domínio, se necessário no futuro
- Demonstrar práticas avançadas de sistemas distribuídos (resiliência, mensageria, sagas)

**Fica mais difícil:**
- Operação local requer Docker Compose com múltiplos contêineres (SQL Server, RabbitMQ, Redis, MailHog + 5 serviços)
- Consistência de dados exige mecanismos explícitos (eventos de domínio, TTL de reserva, polling de expiração)
- Rastreamento de uma transação ponta a ponta requer observabilidade distribuída (correlationId, tracing)
- Testes de integração são mais complexos, exigindo orquestração de múltiplos serviços ou mocks de fronteira
