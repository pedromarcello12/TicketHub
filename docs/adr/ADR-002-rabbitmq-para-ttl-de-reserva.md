# ADR-002: RabbitMQ + Background Worker para TTL de Reserva de Ingressos

## Status
Aceito

## Contexto

O domínio exige que ingressos reservados expirem automaticamente após 10 minutos se o pagamento não for concluído. Isso libera o ingresso de volta ao estoque, evitando que ingressos fiquem presos indefinidamente em estado `Reservado`.

As abordagens avaliadas foram:

1. **RabbitMQ Delayed Messages Plugin**: publicar uma mensagem com delay de 10 minutos no momento da reserva; ao ser consumida, o Worker verifica e expira a reserva.
2. **Background Worker com polling periódico**: um `IHostedService` dentro do Ingressos.Api que consulta periodicamente o banco em busca de reservas expiradas e as libera.
3. **Banco de dados com TTL (ex: Redis EXPIRE)**: armazenar a reserva no Redis com expiração automática; ao expirar, um listener ou polling captura a mudança.

## Decisão

Adotamos o **Background Worker com polling periódico** (`LiberacaoReservaExpiradaWorker`) hospedado dentro do próprio Ingressos.Api. O worker executa em intervalos configuráveis, consulta reservas cujo `ReservadoAte < DateTime.UtcNow` e chama `LiberarReservaExpirada()` em cada agregado.

A lógica de expiração está encapsulada no domínio: `Ingresso.LiberarReservaExpirada(DateTime agora)` verifica as pré-condições e aplica a transição de estado — garantindo que as regras de negócio não vazem para a infraestrutura.

O RabbitMQ permanece no sistema exclusivamente para **eventos de integração assíncronos** entre serviços (ex: `PagamentoStatusAlteradoEvent`), não para mensagens temporizadas.

## Consequências

**Fica mais fácil:**
- Sem dependência do Delayed Messages Plugin do RabbitMQ, que não está habilitado por padrão e exige configuração extra
- O worker é simples, testável e co-localizado com o serviço que possui o domínio
- A regra de expiração vive no agregado `Ingresso`, garantindo integridade pelo domínio
- Resiliente a reinícios: ao subir, o worker imediatamente processa reservas que expiraram enquanto o serviço estava offline

**Fica mais difícil:**
- Granularidade de expiração depende do intervalo de polling (não é exatamente no segundo 600)
- Sob alto volume, a query de polling pode se tornar custosa — exige índice em `Status` e `ReservadoAte`
- Se o Ingressos.Api tiver múltiplas réplicas, múltiplos workers executarão simultaneamente, podendo processar as mesmas reservas. Mitigação atual: a operação é idempotente (verificação de estado antes da transição), mas pode exigir lock otimista ou uso de `SELECT ... WITH (UPDLOCK)` em escala
