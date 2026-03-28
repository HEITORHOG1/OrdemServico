---
name: criar-feature
description: Cria uma feature completa seguindo Clean Architecture - da entidade ate o endpoint e testes. Use quando o usuario pedir para criar uma funcionalidade nova no sistema.
disable-model-invocation: true
user-invocable: true
argument-hint: "[nome-da-feature]"
---

# Criar Feature Completa — $ARGUMENTS

Crie uma feature completa chamada **$ARGUMENTS** seguindo rigorosamente a Clean Architecture do projeto. Execute cada etapa na ordem correta.

## Etapa 1: Domain (src/Domain/)

1. **Entidade** em `Entities/`: classe `sealed`, construtor privado para EF, factory method `Criar(...)`, propriedades com `private set`, `UpdatedAt = DateTime.UtcNow` em metodos mutantes.
2. **Value Objects** em `ValueObjects/` se necessario: `sealed record` com validacao no construtor.
3. **Enums** em `Enums/` se necessario.
4. **Interface do repositorio** em `Interfaces/`: `IXxxRepository` com metodos async em portugues (`AdicionarAsync`, `ObterPorIdAsync`, `ListarPaginadoAsync`, `ContarAsync`, `AtualizarAsync`).
5. **Excecoes** em `Exceptions/` se necessario: herdar de `DomainException`.

## Etapa 2: Application (src/Application/)

1. **DTOs** em `DTOs/{Feature}/`:
   - `CriarXxxRequest` (positional record)
   - `AtualizarXxxRequest` (positional record)
   - `XxxResponse` (positional record)
   - `XxxResumoResponse` para listagens (positional record)
2. **Validators** em `DTOs/{Feature}/Validators/`:
   - `CriarXxxValidator : AbstractValidator<CriarXxxRequest>`
   - `AtualizarXxxValidator : AbstractValidator<AtualizarXxxRequest>`
3. **Mappings** em `DTOs/{Feature}/Mappings/`:
   - Extension methods `ToResponse()`, `ToResumoResponse()` — NUNCA AutoMapper.
4. **Service interface** em `Interfaces/`: `IXxxService` com todos os use cases.
5. **Service implementation** em `Services/`: `XxxService` com injecao de repositorio + UnitOfWork, `CancellationToken` em todos os metodos, helpers `GetXxxOuThrow()` e `Guardar()`.
6. **Registrar no DI** em `DependencyInjection.cs`: `services.AddScoped<IXxxService, XxxService>()`.

## Etapa 3: Infrastructure (src/Infrastructure/)

1. **Repositorio** em `Persistence/Repositories/`: implementar `IXxxRepository` usando `AppDbContext`. Usar `.Include()`, `.AsNoTracking()`, `.AsSplitQuery()` conforme padrao.
2. **EF Configuration** em `Persistence/Configurations/`: `XxxConfiguration : IEntityTypeConfiguration<Xxx>`. Tabela em snake_case, `OwnsOne` para value objects, backing field access mode para colecoes.
3. **DbSet** em `AppDbContext`: adicionar `public DbSet<Xxx> NomePlural { get; set; }`.
4. **Registrar no DI** em `DependencyInjection.cs`: `services.AddScoped<IXxxRepository, XxxRepository>()`.

## Etapa 4: API (src/Api/)

1. **Endpoints** em `Endpoints/XxxEndpoints.cs`:
   - Extension method `MapXxxEndpoints(this IEndpointRouteBuilder routes)`
   - `MapGroup("/api/xxx")` com `.WithTags()`
   - POST para criacao com `ValidationFilter<T>`, retornando `Results.Created()`
   - GET `/{id:guid}` retornando `Results.Ok()` ou `Results.NotFound()`
   - GET `/` paginado com `[AsParameters] PagedRequest`
   - PUT `/{id:guid}` com `ValidationFilter<T>`, retornando `Results.NoContent()`
   - Sempre declarar `.Produces<T>()`, `.ProducesValidationProblem()`, `.ProducesProblem()`
2. **Registrar** em `WebApplicationExtensions.cs`: chamar `app.MapXxxEndpoints()`.

## Etapa 5: Testes

1. **Domain.UnitTests**: testar factory method, regras de negocio, guard clauses, transicoes de estado. Sem mocking.
2. **Application.UnitTests**: mock do repositorio e UnitOfWork com Moq. Testar service methods.

## Checklist Final

- [ ] Todas as classes de implementacao sao `sealed`
- [ ] DTOs sao `record`
- [ ] Sem warnings (TreatWarningsAsErrors=true)
- [ ] Nullable habilitado e tratado
- [ ] CancellationToken em todos os metodos async
- [ ] UnitOfWork.CommitAsync() apos toda mutacao
- [ ] Nomes em portugues para codigo de negocio
- [ ] Build passa: `dotnet build OrdemServico.sln`
- [ ] Testes passam: `dotnet test OrdemServico.sln`
