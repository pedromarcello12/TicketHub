# ADR-005: Polly para Resiliência em Chamadas HTTP entre Serviços

## Status
Aceito

## Contexto

O Ingressos.Api chama o Eventos.Api para validar se um evento existe antes de criar um ingresso. O Pagamento.Api chama o Ingressos.Api para validar se um ingresso existe antes de processar um pagamento. Essas chamadas HTTP síncronas introduzem dependências de disponibilidade entre serviços — se o serviço chamado estiver lento ou indisponível, o serviço chamador também degrada.

Sem políticas de resiliência, falhas transientes (timeouts, picos de latência, reinícios de contêiner) se propagam para cima na cadeia de chamadas e causam falhas em cascata.

As abordagens avaliadas foram:
- **Sem resiliência**: simples, mas frágil em ambiente distribuído
- **Retry manual**: código de retry escrito pelo desenvolvedor, propenso a erros e inconsistências
- **Polly via `Microsoft.Extensions.Http.Resilience`**: integrado nativamente ao `IHttpClientFactory` no .NET 8+, com políticas padrão de retry, circuit breaker e timeout

## Decisão

Adotamos **Polly via `AddStandardResilienceHandler`** do pacote `Microsoft.Extensions.Http.Resilience`, aplicado nos `HttpClient`s que realizam chamadas entre serviços.

As políticas configuradas (em `ResilienciaHttpConfiguracao`) são:

| Política | Configuração |
|---|---|
| Timeout por tentativa | 2 segundos |
| Retry | 2 tentativas, backoff exponencial, delay inicial de 200ms |
| Circuit Breaker | Janela de amostragem de 4 segundos |
| Timeout total | 5 segundos |

O Circuit Breaker abre automaticamente quando a taxa de falhas ultrapassa o limiar, interrompendo chamadas ao serviço defeituoso por um período de recuperação — evitando que erros em cascata sobrecarreguem um serviço já degradado.

## Consequências

**Fica mais fácil:**
- Falhas transientes são absorvidas automaticamente com retry exponencial
- Circuit Breaker isola falhas, evitando degradação em cascata
- Configuração centralizada em `ResilienciaHttpConfiguracao`, aplicada a todos os clientes HTTP do serviço
- Integração nativa com `IHttpClientFactory` — sem necessidade de wrapper manual

**Fica mais difícil:**
- O timeout total de 5 segundos define o pior caso de latência percebida pelo cliente para operações que dependem de outro serviço
- Retry pode causar efeitos colaterais em operações não idempotentes — as chamadas atuais (`ExisteAsync`) são GET/leitura, portanto seguras para retry
- O Circuit Breaker em memória não é compartilhado entre réplicas do mesmo serviço — cada instância mantém seu próprio estado do breaker
