# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**OrdemServico** — A service order (OS) management system built with .NET 9 Clean Architecture. It tracks equipment repair/service orders through a multi-step workflow, managing clients, equipment, services, products, payments, and annotations.

Language: Portuguese (BR) — all domain terms, enums, variable names, and documentation are in Portuguese.

## Build & Run Commands

```bash
# Build entire solution
dotnet build OrdemServico.sln

# Run API (port 8080)
dotnet run --project src/Api

# Run Blazor Web (port 8082, requires API running)
dotnet run --project src/Web

# Run all tests
dotnet test OrdemServico.sln

# Run specific test project
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests
dotnet test tests/Web.UnitTests
dotnet test tests/Api.IntegrationTests

# Run a single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Docker (full stack: API + Web + MySQL + Redis + phpMyAdmin)
docker-compose up --build
```

## Architecture

Clean Architecture with 5 layers. Dependency flows inward: Api/Web -> Application -> Domain. Infrastructure implements Domain interfaces.

- **Domain** — Pure domain model, zero dependencies. Rich entities (not anemic), value objects, repository interfaces, domain exceptions.
- **Application** — Service classes orchestrating use cases, DTOs, FluentValidation validators, manual mapping extensions. No MediatR/CQRS (ADR-003). No AutoMapper (ADR-004).
- **Infrastructure** — EF Core + Pomelo MySQL provider, Redis caching (StackExchange.Redis), repository implementations, UnitOfWork.
- **Api** — Minimal APIs (no controllers). Endpoints grouped by feature via `MapGroup()`. Global exception handler maps to ProblemDetails (RFC 7807). Optional API key auth middleware.
- **Web** — Blazor Server with MVVM pattern using Radzen.Blazor components. ViewModels inherit `ViewModelBase` with INotifyPropertyChanged. Consumes API via typed HTTP wrappers.

## Key Architecture Decisions

- **Sealed classes by default** (ADR-009) — all non-inherited classes should be `sealed` for JIT optimization.
- **No lazy loading** (ADR-011) — explicit `.Include()` required; prevents N+1 queries.
- **GUIDs as CHAR(36) PKs** (ADR-008) — client-side generation.
- **Manual DTO mapping** via extension methods (`ToResponse()`, `ToDomain()`) — no AutoMapper.
- **FluentValidation** for all request DTOs, executed via `ValidationFilter<T>` in API pipeline.
- **TreatWarningsAsErrors=true** globally via Directory.Build.props.

## Domain Workflow (StatusOS)

```
Rascunho -> Orcamento -> Aprovada -> EmAndamento <-> AguardandoPeca -> Concluida -> Entregue
                      -> Rejeitada
```

Status transitions are enforced by `OrdemServico.EfetuarTransicaoWorkflow()`. Invalid transitions throw `StatusTransicaoInvalidaException`.

## Key Value Objects

- **Dinheiro** — Non-negative monetary value with 2-decimal precision, supports arithmetic operators.
- **NumeroOS** — Format `OS-YYYYMMDD-XXXX`, daily sequential numbering.
- **Desconto** — Supports percentage or fixed value (`TipoDesconto` enum), validates against subtotal.

## Infrastructure Details

- **Database**: MySQL 8.0, connection string key `DefaultConnection`
- **Cache**: Redis 7, connection string key `RedisConnection` (optional, graceful fallback)
- **EF Core configs**: Fluent API in separate `IEntityTypeConfiguration<T>` classes under `Infrastructure/Configurations/`
- **Split queries** used for aggregate root loading (`.AsSplitQuery()`)

## Testing

- **Domain.UnitTests** — Pure domain logic, no mocking needed.
- **Application.UnitTests** — Moq for repository/UoW mocking.
- **Web.UnitTests** — Moq for API service mocking, tests ViewModels.
- **Api.IntegrationTests** — Bogus for test data generation.

## Docker Services

| Service    | Port | Purpose              |
|------------|------|----------------------|
| api        | 8080 | .NET API             |
| web        | 8082 | Blazor Web           |
| mysql      | 3306 | MySQL 8.0            |
| redis      | 6379 | Cache                |
| phpmyadmin | 8081 | DB management UI     |

## Documentation

Architecture docs and business rules are in `doc/` — notably `arquitetura_api.md` (full architecture guide with ADRs) and `ordem_servico_regras_negocio.md` (business rules).

---

## Senior .NET Developer Skill

You are a **Senior .NET Developer** with deep expertise in Clean Architecture, Domain-Driven Design, and modern .NET 9 patterns. Follow every convention below strictly — they are derived from the existing codebase and represent the project standard.

### Language & Naming

- **All business code in Portuguese (BR)**: entity names, properties, methods, DTOs, enums, validation messages, exceptions. Only technical/infrastructure terms stay in English (Repository, Service, Factory, Configuration, Filter, Middleware).
- **PascalCase** for classes, methods, properties, enums.
- **camelCase with `_` prefix** for private fields: `_servicos`, `_unitOfWork`, `_form`.
- **Async suffix** on all async methods: `CriarAsync()`, `ObterPorIdAsync()`, `ListarPaginadoAsync()`.
- **Action verbs in Portuguese** for domain/service methods: `Criar`, `Atualizar`, `Adicionar`, `Obter`, `Listar`, `Registrar`, `Aplicar`, `Marcar`, `Remover`.
- **DTO naming**: suffix `Request` for input (`CriarOrdemServicoRequest`), suffix `Response` for output (`OrdemServicoResponse`, `OrdemServicoResumoResponse`).
- **Validators**: same name as DTO + `Validator` suffix (`CriarOrdemServicoValidator`).
- **File-scoped namespaces** always (`namespace Domain.Entities;` not `namespace Domain.Entities { }`).
- **One class per file**, file name matches class name exactly.

### Class Design

- **`sealed` by default** on all implementation classes (services, repositories, DTOs, value objects, ViewModels, middlewares, filters, configurations). Only omit `sealed` on base classes meant for inheritance (`DomainException`, `ViewModelBase`).
- **`record`** for DTOs (Request/Response) and Value Objects: `public record CriarOrdemServicoRequest(...)`, `public sealed record Dinheiro`.
- **Positional records** (primary constructor) for DTOs. Regular records with explicit properties for value objects that need validation in the constructor.
- **Private parameterless constructor** for EF Core entities: `private OrdemServico() { }`.
- **Static factory methods** for entity creation: `public static OrdemServico Criar(...)` — never public constructors on domain entities.
- **`init` or `private set`** on entity properties — never public setters.

### Domain Layer Patterns

- **Rich Domain Model**: all business rules live inside the entity. Services only orchestrate (load, call entity method, save).
- **Aggregate Root**: `OrdemServico` is the aggregate root. Child entities (`OrdemServicoServico`, `OrdemServicoProduto`, etc.) are only modified through the root.
- **Backing field collections**: private `List<T>` field + public `IReadOnlyCollection<T>` property:

  ```csharp
  private readonly List<OrdemServicoServico> _servicos = new();
  public IReadOnlyCollection<OrdemServicoServico> Servicos => _servicos.AsReadOnly();
  ```

- **Value Objects as `sealed record`**: `Dinheiro`, `NumeroOS`, `Desconto`. Immutable, self-validating in constructor, operator overloads where needed.
- **Guard clauses** at method start: validate inputs with `throw new ArgumentException(...)` for invalid arguments, `throw new DomainException(...)` for business rule violations.
- **Workflow transitions**: dedicated methods per transition (`Aprovar()`, `Rejeitar()`, `IniciarAndamento()`) that internally call `EfetuarTransicaoWorkflow()`.
- **`UpdatedAt = DateTime.UtcNow`** at the end of every mutating method.
- **Exception hierarchy**: `DomainException` (base, not sealed) -> `StatusTransicaoInvalidaException`, `DescontoExcedeTotalException`, `ConcurrencyConflictException`.
- **`default!` pattern** for EF-managed required navigation properties: `public NumeroOS Numero { get; private set; } = default!;`.
- **`string.Empty` initialization** for required string properties: `public string Defeito { get; private set; } = string.Empty;`.

### Application Layer Patterns

- **Service interface + implementation**: `IOrdemServicoService` / `OrdemServicoService`. One service per aggregate root.
- **Constructor injection** with `readonly` fields. All dependencies via DI.
- **CancellationToken** as last parameter on all async methods, default `= default`.
- **UnitOfWork pattern**: always call `_unitOfWork.CommitAsync(ct)` after repository mutations. Never `SaveChanges` directly.
- **Private helper methods** to reduce repetition: `GetOsOuThrow()` for load-or-throw, `Guardar()` for update+commit.
- **Idempotency checks** before mutations (e.g., duplicate payment detection, same-status transition = no-op).
- **Optimistic concurrency**: `ValidateConcurrency()` comparing `ExpectedUpdatedAt` with persisted `UpdatedAt`.
- **Manual mapping extensions** in `DTOs/{Feature}/Mappings/` folder: `public static OrdemServicoResponse ToResponse(this OrdemServico os)`.
- **FluentValidation** validators in `DTOs/{Feature}/Validators/` folder. Register via `AddValidatorsFromAssembly()`.
- **`PagedRequest`/`PagedResponse<T>`** for all paginated endpoints.

### Infrastructure Layer Patterns

- **EF Core Fluent API** configurations in separate `IEntityTypeConfiguration<T>` classes under `Persistence/Configurations/`.
- **snake_case table names**: `builder.ToTable("ordens_servico")`.
- **`OwnsOne()`** for value objects mapped to columns: `OwnsOne(x => x.Numero, ...)`, `OwnsOne(x => x.DescontoAplicado, ...)`.
- **Backing field access mode** for aggregate collections:

  ```csharp
  builder.Metadata.FindNavigation(nameof(OrdemServico.Servicos))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);
  ```

- **`DeleteBehavior.Restrict`** for required FK relationships, **`DeleteBehavior.SetNull`** for optional ones.
- **`.Include()` explicitly** for all child collections — never rely on lazy loading.
- **`.AsNoTracking().AsSplitQuery()`** for list/read-only queries.
- **`MarkIfDetachedAsAdded<T>()`** pattern for new aggregate children added through backing fields.
- **Repository methods** in Portuguese: `AdicionarAsync`, `AtualizarAsync`, `ObterPorIdAsync`, `ListarPaginadoAsync`, `ContarAsync`.

### API Layer Patterns

- **Minimal APIs only** — no Controllers, no `[ApiController]`.
- **Static extension methods** for endpoint mapping: `public static void MapOrdemServicoEndpoints(this IEndpointRouteBuilder routes)`.
- **`MapGroup()`** for route grouping with `.WithTags()`.
- **`ValidationFilter<T>`** as endpoint filter for FluentValidation.
- **`[AsParameters]`** for query string binding on GET endpoints.
- **Standard HTTP responses**: `Results.Created()` for POST creation, `Results.NoContent()` for PUT/PATCH/POST mutations, `Results.Ok()` for GET, `Results.NotFound()` for missing resources.
- **ProblemDetails (RFC 7807)** via `GlobalExceptionHandler`:
  - `DomainException` / `ArgumentException` -> 422
  - `ConcurrencyConflictException` -> 409
  - `KeyNotFoundException` -> 404
  - Unhandled -> 500
- **Correlation ID** propagation via `X-Correlation-Id` header middleware.
- **Endpoint metadata**: always declare `.Produces<T>()`, `.ProducesValidationProblem()`, `.ProducesProblem()` for OpenAPI/Swagger.
- **RESTful route naming**: `/api/ordens-servico`, `/api/ordens-servico/{id:guid}/servicos`, `/api/ordens-servico/{id:guid}/status`.

### Web Layer (Blazor MVVM) Patterns

- **ViewModels inherit `ViewModelBase`** (abstract, implements `INotifyPropertyChanged`).
- **`SetProperty<T>(ref T field, T value)`** for all property changes that need UI notification.
- **State management methods**: `SetLoadingState()`, `SetSubmittingState()`, `SetErrorState(message)`, `SetSuccessState(message)`.
- **`ViewState` enum**: `Loading`, `Submitting`, `Error`, `Success`.
- **`OperationResult`** return type for ViewModel operations — wraps success/failure with message and optional validation errors.
- **Flow state objects** (`ClienteFlowState`, `EquipamentoFlowState`, `OrdemServicoFlowState`) for cross-page state.
- **`ApiResult<T>`** wrapper from API client — always check `.Succeeded` before accessing `.Data`.
- **JSON deserialization** via `System.Text.Json` with `JsonSerializerDefaults.Web` options.
- **Typed API wrappers** (e.g., `IOrdensServicoApi`, `IClientesApi`) that wrap `IApiClient` generic calls.
- **Form models** as separate classes in ViewModels folder (`OrdemServicoCadastroFormModel`), bound to Blazor forms.
- **Client-side validation** before API call: `ValidarFormulario()` returns `Dictionary<string, string[]>`.
- **`NormalizarCampo()`** pattern: trim whitespace, convert empty to null.

### Testing Patterns

- **xUnit** as test framework, **Moq** for mocking (Application/Web layers).
- **Bogus** for test data generation in integration tests.
- **Domain tests**: pure, no mocking — test entity behavior directly via factory methods.
- **Application tests**: mock repositories and UnitOfWork, verify service orchestration.
- **Web tests**: mock API service interfaces, test ViewModel state transitions.
- **Test method naming**: descriptive in Portuguese or English, using pattern `MetodoSobTeste_Cenario_ResultadoEsperado`.

### Code Quality Rules

- **TreatWarningsAsErrors=true** — zero warnings allowed. Fix warnings, don't suppress them (except documented cases like `CA1848`).
- **Nullable reference types enabled** — handle nullability explicitly. Use `is not null`, `is null`, `?.`, `??`, `default!`.
- **Pattern matching** preferred: `is StatusOS.Rascunho or StatusOS.Orcamento`, `exception is DomainException or ArgumentException`.
- **Expression-bodied members** for single-line methods and properties: `public Dinheiro CalcularTotalReal() => ...;`.
- **No `this.`** qualifier unless disambiguating.
- **No regions** (`#region`).
- **Implicit typing (`var`)** when type is obvious from right side.
- **Collection expressions** where appropriate: `["error message"]` for single-element arrays.
- **`string.IsNullOrWhiteSpace()`** always preferred over `string.IsNullOrEmpty()`.
- **File-scoped usings** via `<ImplicitUsings>enable</ImplicitUsings>` — avoid explicit `using System;`, `using System.Collections.Generic;`, etc.
- **No magic numbers/strings** in business logic — use constants, enums, or Value Objects.
- **Decimal precision**: always `Math.Round(value, 2)` for monetary calculations.

### Dependency Rules (Strict)

- **Domain**: references NOTHING. Zero project references, zero NuGet packages.
- **Application**: references only Domain. NuGet: FluentValidation only.
- **Infrastructure**: references Domain and Application. NuGet: EF Core, Pomelo MySQL, StackExchange.Redis.
- **Api**: references Application and Infrastructure. NuGet: Swashbuckle.
- **Web**: references nothing from Domain/Application/Infrastructure directly — communicates only via HTTP to the API.
- **Tests**: can reference the layer they test + its dependencies.

### When Creating New Features

1. Start in **Domain**: create/modify entities, value objects, enums, repository interfaces, exceptions.
2. Move to **Application**: create DTOs (records), validators, mapping extensions, service interface + implementation.
3. Wire **Infrastructure**: repository implementation, EF configuration, DI registration.
4. Expose via **Api**: new endpoint method in the feature's `*Endpoints.cs` file with filters and metadata.
5. Consume from **Web**: API wrapper method, ViewModel, FormModel, Razor page.
6. Write **tests** at each layer.

### When Modifying Existing Features

- Read the existing code first. Understand the entity's state machine and business rules before changing anything.
- Respect the aggregate boundary — never modify child entities directly, always through the aggregate root.
- Update `UpdatedAt` in every mutating domain method.
- If adding a new child collection, follow the full backing field pattern + EF configuration + `MarkIfDetachedAsAdded()`.
- If adding a new status transition, add the method to `OrdemServico` and update `AlterarStatusAsync()` switch.

### Performance Guidelines

- **`.AsSplitQuery()`** on queries loading multiple collections (prevents cartesian explosion).
- **`.AsNoTracking()`** on read-only/list queries.
- **Sealed classes** for JIT devirtualization.
- **No lazy loading** — every needed Include must be explicit.
- **Redis caching** for frequently-read, rarely-changed data via `ICacheService`.
- **Pagination** on all list endpoints — never return unbounded collections.
