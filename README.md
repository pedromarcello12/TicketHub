# TicketHub

Plataforma de venda de ingressos para eventos, construída como um conjunto de microserviços em .NET.

## Arquitetura

| Serviço | Responsabilidade | Porta (docker compose) |
|---|---|---|
| `Auth.Api` | Autenticação (login, registro, refresh token), emissão de JWT | 5004 |
| `Eventos.Api` | Cadastro e ciclo de vida de eventos (planejar, publicar, cancelar, encerrar) | 5001 |
| `Ingressos.Api` | Emissão e reserva de ingressos | 5002 |
| `Pagamento.Api` | Processamento de pagamento de ingressos | 5003 |
| `Notificacoes.Worker` | Consome eventos de mudança de status de pagamento e envia e-mail ao cliente | — (worker em background) |

Cada serviço de API segue Clean Architecture (`*.Domain`, `*.Application`, `*.Infrastructure`, `*.Api`) e tem seu próprio banco SQL Server. A comunicação entre serviços é majoritariamente HTTP síncrono (ex.: `Ingressos.Api` valida o evento chamando `Eventos.Api`), autenticado via um usuário de serviço interno. `Pagamento.Api` publica eventos assíncronos no RabbitMQ, consumidos por `Notificacoes.Worker`.

Building blocks compartilhados em `src/BuildingBlocks`:
- **TicketHub.Core** — exceções e utilitários comuns.
- **TicketHub.Auth** — emissão/validação de JWT, rate limiting, cliente HTTP autenticado para chamadas serviço-a-serviço.
- **TicketHub.MessageBus** — publisher/consumer base para RabbitMQ.

## Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core + SQL Server
- RabbitMQ (mensageria assíncrona)
- xUnit + NSubstitute + FluentAssertions (testes)
- Docker Compose (ambiente local)
- GitHub Actions (CI)

## Rodando localmente com Docker Compose

Pré-requisitos: Docker e Docker Compose.

```bash
cp .env.example .env
# edite .env e defina JWT_SECRET_KEY e SERVICO_INTERNO_SENHA (veja docs/SECRETS.md)

docker compose up -d
```

Isso sobe SQL Server, RabbitMQ, Redis, MailHog (SMTP fake com UI em `http://localhost:8025`) e os 5 serviços da aplicação. Cada API aplica suas migrations automaticamente no startup.

APIs disponíveis em `http://localhost:<porta>` conforme a tabela acima (ex.: `http://localhost:5004/openapi/v1.json` para o documento OpenAPI do `Auth.Api`).

## Rodando sem Docker (`dotnet run`)

Requer .NET 10 SDK, e um SQL Server / RabbitMQ acessíveis localmente (ajuste as connection strings em cada `appsettings.Development.json` ou via User Secrets).

```bash
cd src/Services/Auth/Auth.Api
dotnet user-secrets set "Jwt:SecretKey" "sua-chave-aqui"
dotnet run
```

Repita para os demais serviços — veja `docs/SECRETS.md` para detalhes sobre segredos compartilhados entre serviços (a mesma `Jwt:SecretKey` precisa estar configurada em todos).

## Testes

```bash
dotnet test TicketHub.slnx
```

## Fluxo de branches e CI

Este projeto usa Trunk-Based Development — veja [`docs/TRUNK_BASED.md`](docs/TRUNK_BASED.md). Todo push/PR contra `master` roda build + testes automaticamente via GitHub Actions (`.github/workflows/ci.yml`).

## Documentação adicional

- [`docs/SECRETS.md`](docs/SECRETS.md) — segredos e configuração local.
- [`docs/TRUNK_BASED.md`](docs/TRUNK_BASED.md) — fluxo de branches.
