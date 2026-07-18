# Fluxo de branches (Trunk-Based Development)

Este projeto usa Trunk-Based Development: uma única branch principal (`master`, o "trunk"), sem `develop`, `release/*` ou `hotfix/*`.

## Branch principal

- `master` — sempre em estado deployável. É a única branch de longa duração do repositório.

## Branches de trabalho

- Toda mudança nasce de uma branch curta criada a partir de `master`:
  ```
  git checkout master
  git pull
  git checkout -b minha-mudanca
  ```
- Nome da branch descreve a mudança, em kebab-case (`checkout-ingressos`, `corrige-preco-negativo`). Não há mais prefixos obrigatórios (`feature/`, `hotfix/`) — a branch é só um meio para o PR, não uma categoria.
- A branch deve viver **pouco tempo** (idealmente menos de 1–2 dias) e ser integrada de volta via PR assim que possível. Evite acumular várias features em uma mesma branch.
- Se a mudança for grande, prefira quebrá-la em PRs menores e incrementais em vez de manter uma branch longa. Use feature flags quando precisar mergear código ainda não pronto para uso em produção.

## Regras

- Nunca commitar diretamente em `master`; sempre via PR.
- O CI deve passar (build + testes) antes do merge — é o que garante que o trunk permanece deployável a qualquer momento.
- Faça merge (ou squash) para `master` assim que o PR for aprovado e o CI passar; não deixe PRs aprovados acumulando.
- Correções urgentes seguem o mesmo caminho: branch curta a partir de `master`, PR, merge — sem branch `hotfix/*` separada.
- Após merge relevante em `master`, criar uma tag com a versão quando aplicável a um release.
