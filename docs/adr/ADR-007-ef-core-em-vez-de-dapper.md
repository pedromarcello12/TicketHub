# ADR-007: EF Core em vez de Dapper para Acesso a Dados

## Status
Aceito — substitui decisão anterior

## Contexto

O guia de construção original previa o uso do **Dapper** para acesso a dados performático: SQL explícito com mapeamento leve, sem o overhead do ORM completo. Essa escolha faz sentido em cenários de alta performance com queries complexas e controle total do SQL.

Na prática, o desenvolvimento do TicketHub priorizou:
- Velocidade de desenvolvimento (migrations automáticas, mapeamento sem SQL manual)
- Modelagem de domínio rica (agregados com construtores privados, value objects, invariantes encapsuladas)
- Rastreamento de mudanças automático para persistir transições de estado de agregados

As abordagens avaliadas foram:
1. **Dapper**: SQL explícito, performance máxima, mas sem rastreamento de mudanças — exige SQL de UPDATE manual para cada transição de estado de agregado
2. **EF Core**: ORM completo com migrations, rastreamento automático de mudanças, Fluent API para mapeamento de agregados DDD

## Decisão

Adotamos **EF Core** em todos os serviços. O mapeamento de cada agregado é feito via Fluent API em classes de configuração separadas (`EventoConfiguracao`, `IngressoConfiguracao`, etc.), mantendo o domínio limpo de atributos de infraestrutura.

Características do uso:
- **Construtores privados** nos agregados (requerido pelo EF Core via constructor sem parâmetros `private`)
- **Fluent API** para configurar tabelas, colunas, conversores de enum e constraints
- **Migrations gerenciadas por serviço**, cada um com seu próprio `DbContext` e histórico de migrations
- **`SaveChangesAsync`** como unidade de trabalho implícita por operação de domínio

## Consequências

**Fica mais fácil:**
- Migrations automatizadas: `dotnet ef migrations add` gera o SQL de schema evolution
- Rastreamento de mudanças: transições de estado (`ingresso.Reservar()`, `pagamento.Aprovar()`) são detectadas automaticamente e persistidas sem SQL manual
- Mapeamento de domínio rico: propriedades privadas, conversores de enum, value objects
- Relacionamentos e navegação expressos em C#, sem JOIN manual

**Fica mais difícil:**
- Performance em queries de leitura com alto volume pode ser inferior ao Dapper — mitigável com `AsNoTracking()` nas queries de leitura
- O overhead do ORM é desnecessário em consultas analíticas complexas — nesses casos, Dapper pode ser introduzido pontualmente no futuro
- O modelo de domínio precisa ter construtores sem parâmetros (privados) para o EF Core conseguir materializar entidades, o que é um detalhe de infraestrutura vazando para o domínio
