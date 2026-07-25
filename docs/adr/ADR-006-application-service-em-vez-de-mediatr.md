# ADR-006: Application Service Direto em vez de MediatR/CQRS com Handlers

## Status
Aceito

## Contexto

O guia de construção do TicketHub originalmente previa o uso do **MediatR** para implementar o padrão CQRS: cada operação seria representada por um `IRequest<T>` e processada por um `IRequestHandler<TRequest, TResponse>`, com o controller emitindo comandos/queries para o mediador.

As abordagens avaliadas foram:

1. **MediatR com Commands/Queries**: desacopla controllers de handlers, facilita pipeline behaviors (logging, validação, retry), mas adiciona indireção e uma dependência extra
2. **Application Service direto**: interface de serviço (`IEventoAppService`) injetada diretamente no controller, sem mediador intermediário

## Decisão

Adotamos **Application Services diretamente injetados nos controllers**, sem MediatR. Cada serviço tem uma interface de aplicação (`IEventoAppService`, `IIngressoAppService`, `IPagamentoAppService`, `IAuthAppService`) implementada por uma classe de serviço concreta.

Essa decisão foi tomada porque:
- O domínio do TicketHub, embora rico em regras de negócio, tem operações de leitura e escrita que não justificam a complexidade do pipeline MediatR neste estágio
- A separação de responsabilidades já é garantida pela arquitetura em camadas (Domain, Application, Infrastructure, API)
- Adicionar MediatR sem pipeline behaviors ativos seria indireção sem benefício real

O CQRS conceitual ainda está presente: métodos de leitura (`ObterPorIdAsync`, `ListarAsync`) são separados dos métodos de escrita (`CriarAsync`, `ReservarAsync`, `AprovarAsync`), mas sem segregação física de modelos de leitura/escrita.

## Consequências

**Fica mais fácil:**
- Código mais direto e rastreável: controller → service → repository, sem saltar entre handlers
- Menos dependências: sem o pacote MediatR
- Testes de unidade mais simples: mock direto da interface do serviço

**Fica mais difícil:**
- Adicionar cross-cutting concerns (logging estruturado, validação, retry em nível de comando) exige decorators manuais ou middleware, em vez de pipeline behaviors do MediatR
- Se o sistema crescer para exigir CQRS completo com read models otimizados, a refatoração para MediatR ou outro mediador será necessária
- Não há separação física entre modelos de leitura e escrita — ambos retornam o mesmo `Response` DTO
