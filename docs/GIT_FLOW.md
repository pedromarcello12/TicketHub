# Fluxo de branches (Git Flow)

Este projeto segue o modelo Git Flow, sem depender da extensão `git-flow` — apenas convenção manual com comandos `git` padrão.

## Branches principais

- `master` — código em produção. Só recebe merge de `release/*` ou `hotfix/*`.
- `develop` — branch de integração. Todo o trabalho concluído de features passa por aqui antes de virar release.

## Branches de apoio

- `feature/<nome>` — criada a partir de `develop`, mergeada de volta em `develop` via PR.
  ```
  git checkout develop
  git checkout -b feature/nome-da-feature
  ```
- `release/<versao>` — criada a partir de `develop` quando um conjunto de features está pronto para lançamento. Mergeada em `master` e `develop`.
  ```
  git checkout develop
  git checkout -b release/1.0.0
  ```
- `hotfix/<nome>` — criada a partir de `master` para correções urgentes em produção. Mergeada em `master` e `develop`.
  ```
  git checkout master
  git checkout -b hotfix/corrige-x
  ```

## Regras

- Nunca commitar diretamente em `master` ou `develop`; sempre via PR.
- Nome da branch descreve a mudança, em kebab-case (`feature/checkout-ingressos`, `hotfix/preco-negativo`).
- Após merge de `release/*` ou `hotfix/*` em `master`, criar uma tag com a versão.
