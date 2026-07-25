# Deploy — Azure Container Apps

Este documento descreve como provisionar a infraestrutura no Azure e configurar os secrets necessários para o pipeline de deploy automático.

---

## Pré-requisitos

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) instalado
- Conta Azure com permissão de Contributor no subscription
- [GitHub CLI](https://cli.github.com/) (opcional, para configurar secrets via terminal)

---

## 1. Criar Service Principal (identidade do GitHub Actions)

```bash
az login

SUBSCRIPTION_ID=$(az account show --query id -o tsv)

az ad sp create-for-rbac \
  --name "sp-tickethub-github" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth
```

Anote o output JSON. Você precisará de `clientId`, `tenantId` e `subscriptionId`.

---

## 2. Provisionar a infraestrutura base

```bash
RESOURCE_GROUP=rg-tickethub
LOCATION=brazilsouth
ACR_NAME=acrtickethub           # deve ser globalmente único
ENVIRONMENT=env-tickethub

# Resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Azure Container Registry
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Container Apps Environment
az containerapp env create \
  --name $ENVIRONMENT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# SQL Server (Azure SQL)
az sql server create \
  --name sql-tickethub \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user sqladmin \
  --admin-password "<senha-forte>"

az sql db create --resource-group $RESOURCE_GROUP --server sql-tickethub --name TicketHubAuth     --tier Basic
az sql db create --resource-group $RESOURCE_GROUP --server sql-tickethub --name TicketHubEventos  --tier Basic
az sql db create --resource-group $RESOURCE_GROUP --server sql-tickethub --name TicketHubIngressos --tier Basic
az sql db create --resource-group $RESOURCE_GROUP --server sql-tickethub --name TicketHubPagamento --tier Basic

# Firewall: permite acesso dos Container Apps (0.0.0.0 permite serviços Azure)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server sql-tickethub \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# RabbitMQ via Container App (ou use CloudAMQP free tier)
az containerapp create \
  --name ca-tickethub-rabbitmq \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image rabbitmq:3-management \
  --target-port 5672 \
  --ingress internal \
  --cpu 0.5 --memory 1Gi

# Azure Cache for Redis (usado pelo Eventos.Api)
az redis create \
  --resource-group $RESOURCE_GROUP \
  --name redis-tickethub \
  --location $LOCATION \
  --sku Basic \
  --vm-size C0

# Aguarde o provisionamento (~15 min) e obtenha a connection string:
REDIS_HOST=$(az redis show \
  --resource-group $RESOURCE_GROUP \
  --name redis-tickethub \
  --query hostName -o tsv)

REDIS_KEY=$(az redis list-keys \
  --resource-group $RESOURCE_GROUP \
  --name redis-tickethub \
  --query primaryKey -o tsv)

# Connection string no formato StackExchange.Redis:
# $REDIS_HOST:6380,password=$REDIS_KEY,ssl=True,abortConnect=False
```

---

## 3. Criar os Container Apps dos serviços

```bash
# Auth.Api
az containerapp create \
  --name ca-tickethub-auth \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image $ACR_NAME.azurecr.io/tickethub/auth-api:latest \
  --registry-server $ACR_NAME.azurecr.io \
  --target-port 8080 \
  --ingress external \
  --cpu 0.5 --memory 1Gi \
  --min-replicas 1 --max-replicas 3 \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "Jwt__SecretKey=secretref:jwt-secret-key" \
    "ServicoInterno__Senha=secretref:servico-interno-senha"

# Eventos.Api (com Redis)
az containerapp create \
  --name ca-tickethub-eventos \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image $ACR_NAME.azurecr.io/tickethub/eventos-api:latest \
  --registry-server $ACR_NAME.azurecr.io \
  --target-port 8080 \
  --ingress external \
  --cpu 0.5 --memory 1Gi \
  --min-replicas 1 --max-replicas 3 \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "Jwt__SecretKey=secretref:jwt-secret-key" \
    "ConnectionStrings__Redis=$REDIS_HOST:6380,password=$REDIS_KEY,ssl=True,abortConnect=False"

# Ingressos.Api
az containerapp create \
  --name ca-tickethub-ingressos \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image $ACR_NAME.azurecr.io/tickethub/ingressos-api:latest \
  --registry-server $ACR_NAME.azurecr.io \
  --target-port 8080 \
  --ingress external \
  --cpu 0.5 --memory 1Gi \
  --min-replicas 1 --max-replicas 3 \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "Jwt__SecretKey=secretref:jwt-secret-key" \
    "ServicoInterno__Senha=secretref:servico-interno-senha"

# Pagamento.Api
az containerapp create \
  --name ca-tickethub-pagamento \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image $ACR_NAME.azurecr.io/tickethub/pagamento-api:latest \
  --registry-server $ACR_NAME.azurecr.io \
  --target-port 8080 \
  --ingress external \
  --cpu 0.5 --memory 1Gi \
  --min-replicas 1 --max-replicas 3 \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    "Jwt__SecretKey=secretref:jwt-secret-key" \
    "ServicoInterno__Senha=secretref:servico-interno-senha"

# Frontend (nginx)
az containerapp create \
  --name ca-tickethub-frontend \
  --resource-group $RESOURCE_GROUP \
  --environment $ENVIRONMENT \
  --image $ACR_NAME.azurecr.io/tickethub/frontend:latest \
  --registry-server $ACR_NAME.azurecr.io \
  --target-port 80 \
  --ingress external \
  --cpu 0.25 --memory 0.5Gi \
  --min-replicas 1 --max-replicas 2
```

---

## 4. Configurar secrets no GitHub

No repositório do GitHub, vá em **Settings → Secrets and variables → Actions** e adicione:

| Secret | Descrição |
|---|---|
| `AZURE_CLIENT_ID` | `clientId` do service principal |
| `AZURE_TENANT_ID` | `tenantId` do service principal |
| `AZURE_SUBSCRIPTION_ID` | ID do subscription Azure |
| `JWT_SECRET_KEY` | Chave JWT de produção (mín. 32 chars) |
| `SERVICO_INTERNO_SENHA` | Senha do serviço interno de produção |

---

## 5. Fluxo do pipeline

O pipeline `.github/workflows/deploy.yml` é disparado em cada push para `master`:

1. **Build & Push** (paralelo por serviço) — compila a imagem Docker e envia ao ACR com tag `sha` e `latest`
2. **Deploy** (paralelo por serviço, após build) — atualiza cada Container App com a nova imagem

O rollback é feito via:
```bash
az containerapp revision list --name ca-tickethub-auth --resource-group rg-tickethub
az containerapp revision activate --name ca-tickethub-auth --resource-group rg-tickethub --revision <revision-name>
```
