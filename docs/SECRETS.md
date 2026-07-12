# Segredos locais

A chave de assinatura JWT (`Jwt:SecretKey`) não fica no `appsettings.json` — é a mesma chave compartilhada entre `Auth.Api` (emite o token) e `Eventos.Api`/`Ingressos.Api`/`Pagamento.Api` (validam o token), então não pode ser versionada.

## Rodando via Docker Compose

1. Copie `.env.example` para `.env`:
   ```
   cp .env.example .env
   ```
2. Edite `.env` e defina `JWT_SECRET_KEY` com uma chave forte (ex.: `openssl rand -base64 48`).
3. `docker compose up -d` — o Compose falha com uma mensagem clara se `JWT_SECRET_KEY` não estiver definida.

`.env` está no `.gitignore` e nunca deve ser commitado.

## Rodando localmente (`dotnet run`, fora do Docker)

Cada um dos 4 serviços (`Auth.Api`, `Eventos.Api`, `Ingressos.Api`, `Pagamento.Api`) usa [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), que só é lido em ambiente `Development`:

```
cd src/Services/Auth/Auth.Api
dotnet user-secrets set "Jwt:SecretKey" "sua-chave-aqui"
```

Repita para os outros 3 serviços, sempre com a **mesma chave** (senão a validação do token falha entre serviços).

Os valores ficam fora do repositório (`%APPDATA%\Microsoft\UserSecrets` no Windows). O `<UserSecretsId>` no `.csproj` de cada serviço é só um identificador, não o segredo — pode ser commitado com segurança.
