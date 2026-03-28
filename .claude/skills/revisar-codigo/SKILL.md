---
name: revisar-codigo
description: Revisa codigo C# como um Senior .NET Developer, verificando aderencia aos padroes Clean Architecture, DDD, performance e qualidade do projeto.
disable-model-invocation: true
user-invocable: true
argument-hint: "[arquivo-ou-pasta]"
---

# Code Review Senior .NET — $ARGUMENTS

Revise o codigo em **$ARGUMENTS** como um desenvolvedor .NET Senior. Analise cada ponto abaixo e reporte problemas encontrados com sugestao de correcao.

## Checklist de Revisao

### 1. Clean Architecture

- [ ] Dependencias respeitam a regra: Domain <- Application <- Infrastructure/Api/Web
- [ ] Domain nao referencia nenhum pacote externo
- [ ] Web nao referencia Domain/Application/Infrastructure diretamente
- [ ] Regras de negocio estao na entidade (Rich Model), nao no Service

### 2. Design de Classes

- [ ] Classes de implementacao sao `sealed`
- [ ] DTOs sao `record`
- [ ] Entidades usam factory method (sem construtor publico)
- [ ] Propriedades de entidade com `private set`
- [ ] Colecoes usam backing field + `IReadOnlyCollection`

### 3. Nomenclatura

- [ ] Codigo de negocio em Portugues (BR)
- [ ] Metodos async com sufixo `Async`
- [ ] Campos privados com prefixo `_`
- [ ] File-scoped namespaces
- [ ] DTOs com sufixo Request/Response

### 4. Qualidade

- [ ] Sem warnings (TreatWarningsAsErrors=true)
- [ ] Nullable tratado corretamente (`?.`, `??`, `is not null`)
- [ ] `string.IsNullOrWhiteSpace()` (nao `IsNullOrEmpty`)
- [ ] Sem magic numbers/strings
- [ ] `Math.Round(value, 2)` para valores monetarios
- [ ] Pattern matching usado onde apropriado
- [ ] Sem `#region`
- [ ] Sem `this.` desnecessario

### 5. Domain

- [ ] Guard clauses no inicio dos metodos
- [ ] `UpdatedAt = DateTime.UtcNow` em metodos mutantes
- [ ] Excecoes corretas: `ArgumentException` vs `DomainException`
- [ ] Transicoes de status via metodos dedicados
- [ ] Value Objects imutaveis e auto-validantes

### 6. Application

- [ ] `CancellationToken` em todos os metodos async
- [ ] `UnitOfWork.CommitAsync()` apos mutacoes
- [ ] Idempotencia em operacoes sensiveis
- [ ] Concorrencia otimista onde necessario
- [ ] FluentValidation para DTOs de entrada
- [ ] Mapping manual (sem AutoMapper)

### 7. Infrastructure

- [ ] `.Include()` explicito (sem lazy loading)
- [ ] `.AsNoTracking()` em queries read-only
- [ ] `.AsSplitQuery()` com multiplos Includes
- [ ] `MarkIfDetachedAsAdded()` para filhos novos
- [ ] Tabelas em snake_case
- [ ] Value Objects via `OwnsOne()`

### 8. API

- [ ] Minimal API (sem Controllers)
- [ ] `ValidationFilter<T>` em endpoints com body
- [ ] Metadata `.Produces()` em todos os endpoints
- [ ] ProblemDetails para erros
- [ ] Rotas RESTful em kebab-case

### 9. Web (Blazor)

- [ ] ViewModel herda `ViewModelBase`
- [ ] `SetProperty()` para notificacao
- [ ] State management correto (Loading/Submitting/Error/Success)
- [ ] `ApiResult.Succeeded` verificado antes de `.Data`
- [ ] Validacao local antes de chamada API

### 10. Performance

- [ ] Sealed classes para JIT
- [ ] Sem N+1 queries
- [ ] Paginacao em listagens
- [ ] Cache Redis para dados frequentes

## Formato da Saida

Para cada problema encontrado, reporte:
1. **Arquivo:Linha** — localizacao
2. **Severidade** — Critico / Alto / Medio / Baixo
3. **Problema** — descricao objetiva
4. **Sugestao** — codigo corrigido

Ao final, de uma nota geral (0-10) e um resumo.
