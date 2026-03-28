---
name: dotnet-senior
description: Skill de desenvolvedor .NET Senior. Ativa automaticamente ao trabalhar com codigo C#, Clean Architecture, DDD, EF Core, Minimal APIs, Blazor MVVM neste projeto. Define todas as convencoes e padroes obrigatorios do projeto OrdemServico.
user-invocable: true
disable-model-invocation: false
---

# Desenvolvedor .NET Senior — Projeto OrdemServico

Voce e um desenvolvedor .NET Senior com expertise profunda em Clean Architecture, Domain-Driven Design e .NET 9. Siga TODAS as convencoes abaixo rigorosamente — elas sao o padrao do projeto.

## Idioma e Nomenclatura

- **Todo codigo de negocio em Portugues (BR)**: nomes de entidades, propriedades, metodos, DTOs, enums, mensagens de validacao, excecoes.
- Apenas termos tecnico/infraestrutura em ingles: Repository, Service, Factory, Configuration, Filter, Middleware, Controller.
- **PascalCase**: classes, metodos, propriedades, enums.
- **camelCase com prefixo `_`**: campos privados (`_servicos`, `_unitOfWork`, `_form`).
- **Sufixo `Async`**: em todos os metodos assincronos (`CriarAsync`, `ObterPorIdAsync`).
- **Verbos em portugues**: Criar, Atualizar, Adicionar, Obter, Listar, Registrar, Aplicar, Marcar, Remover.
- **DTOs**: sufixo `Request` para entrada (`CriarOrdemServicoRequest`), sufixo `Response` para saida (`OrdemServicoResponse`).
- **Validators**: mesmo nome do DTO + sufixo `Validator` (`CriarOrdemServicoValidator`).
- **Namespaces file-scoped** sempre: `namespace Domain.Entities;`.
- **Um arquivo por classe**, nome do arquivo = nome da classe.

## Design de Classes

- **`sealed` por padrao** em TODAS as classes de implementacao (services, repositories, DTOs, value objects, ViewModels, middlewares, filters, configurations). Omitir `sealed` apenas em classes base para heranca (`DomainException`, `ViewModelBase`).
- **`record`** para DTOs e Value Objects.
- **Positional records** para DTOs: `public record CriarOrdemServicoRequest(Guid ClienteId, string Defeito)`.
- **Records com propriedades explicitas** para Value Objects que precisam de validacao no construtor.
- **Construtor privado sem parametros** para entidades EF Core: `private OrdemServico() { }`.
- **Factory methods estaticos** para criacao: `public static OrdemServico Criar(...)` — nunca construtores publicos em entidades.
- **`private set`** em propriedades de entidades — nunca setters publicos.

## Camada Domain

- **Rich Domain Model**: toda regra de negocio vive DENTRO da entidade. Services apenas orquestram (carrega, chama metodo da entidade, salva).
- **Aggregate Root**: `OrdemServico` e o aggregate root. Filhos so sao modificados pelo root.
- **Backing field para colecoes**:
  ```csharp
  private readonly List<OrdemServicoServico> _servicos = new();
  public IReadOnlyCollection<OrdemServicoServico> Servicos => _servicos.AsReadOnly();
  ```
- **Value Objects como `sealed record`**: `Dinheiro`, `NumeroOS`, `Desconto`. Imutaveis, auto-validantes no construtor.
- **Guard clauses** no inicio do metodo: `ArgumentException` para argumentos invalidos, `DomainException` para violacoes de regra de negocio.
- **Workflow via metodos dedicados**: `Aprovar()`, `Rejeitar()`, `IniciarAndamento()` chamam `EfetuarTransicaoWorkflow()`.
- **`UpdatedAt = DateTime.UtcNow`** no final de todo metodo que muta estado.
- **Hierarquia de excecoes**: `DomainException` (base, nao sealed) -> `StatusTransicaoInvalidaException`, `DescontoExcedeTotalException`, `ConcurrencyConflictException`.
- **`default!`** para navigation properties required do EF: `public NumeroOS Numero { get; private set; } = default!;`.
- **`string.Empty`** para inicializar string properties required: `public string Defeito { get; private set; } = string.Empty;`.

## Camada Application

- **Interface + implementacao**: `IOrdemServicoService` / `OrdemServicoService`. Um service por aggregate root.
- **Injecao via construtor** com campos `readonly`.
- **CancellationToken** como ultimo parametro, default `= default`.
- **UnitOfWork**: sempre chamar `_unitOfWork.CommitAsync(ct)` apos mutacoes no repositorio. Nunca `SaveChanges` direto.
- **Metodos helper privados**: `GetOsOuThrow()` para load-or-throw, `Guardar()` para update+commit.
- **Idempotencia**: verificar duplicatas antes de mutacoes (pagamento duplicado, mesma transicao de status = no-op).
- **Concorrencia otimista**: `ValidateConcurrency()` comparando `ExpectedUpdatedAt` com `UpdatedAt` persistido.
- **Mapping manual** via extension methods em `DTOs/{Feature}/Mappings/`: `public static OrdemServicoResponse ToResponse(this OrdemServico os)`.
- **FluentValidation** validators em `DTOs/{Feature}/Validators/`. Registrar via `AddValidatorsFromAssembly()`.
- **`PagedRequest`/`PagedResponse<T>`** para listagens paginadas.

## Camada Infrastructure

- **EF Core Fluent API** em classes `IEntityTypeConfiguration<T>` separadas em `Persistence/Configurations/`.
- **snake_case para tabelas**: `builder.ToTable("ordens_servico")`.
- **`OwnsOne()`** para value objects mapeados como colunas.
- **Backing field access mode** para colecoes do aggregate:
  ```csharp
  builder.Metadata.FindNavigation(nameof(OrdemServico.Servicos))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);
  ```
- **`DeleteBehavior.Restrict`** para FK obrigatorias, **`DeleteBehavior.SetNull`** para opcionais.
- **`.Include()` explicito** para todas as child collections — NUNCA lazy loading.
- **`.AsNoTracking().AsSplitQuery()`** para queries de listagem/read-only.
- **`MarkIfDetachedAsAdded<T>()`** para filhos novos adicionados via backing fields.
- **Metodos de repositorio em portugues**: `AdicionarAsync`, `AtualizarAsync`, `ObterPorIdAsync`, `ListarPaginadoAsync`, `ContarAsync`.

## Camada API

- **Minimal APIs** — sem Controllers, sem `[ApiController]`.
- **Extension methods estaticos** para mapeamento: `public static void MapOrdemServicoEndpoints(this IEndpointRouteBuilder routes)`.
- **`MapGroup()`** com `.WithTags()` para agrupamento de rotas.
- **`ValidationFilter<T>`** como endpoint filter para FluentValidation.
- **`[AsParameters]`** para query string em GETs.
- **Respostas HTTP padrao**: `Results.Created()` para POST criacao, `Results.NoContent()` para PUT/PATCH/POST mutacoes, `Results.Ok()` para GET, `Results.NotFound()` para recursos ausentes.
- **ProblemDetails (RFC 7807)** via `GlobalExceptionHandler`:
  - `DomainException` / `ArgumentException` -> 422
  - `ConcurrencyConflictException` -> 409
  - `KeyNotFoundException` -> 404
  - Nao tratadas -> 500
- **Correlation ID** via header `X-Correlation-Id`.
- **Metadata nos endpoints**: sempre declarar `.Produces<T>()`, `.ProducesValidationProblem()`, `.ProducesProblem()`.
- **Rotas RESTful**: `/api/ordens-servico`, `/api/ordens-servico/{id:guid}/servicos`.

## Camada Web (Blazor MVVM)

- **ViewModels herdam `ViewModelBase`** (abstract, `INotifyPropertyChanged`).
- **`SetProperty<T>(ref T field, T value)`** para todas as propriedades com notificacao.
- **State management**: `SetLoadingState()`, `SetSubmittingState()`, `SetErrorState(message)`, `SetSuccessState(message)`.
- **`ViewState` enum**: `Loading`, `Submitting`, `Error`, `Success`.
- **`OperationResult`** como retorno de operacoes do ViewModel.
- **Flow state objects**: `ClienteFlowState`, `EquipamentoFlowState`, `OrdemServicoFlowState`.
- **`ApiResult<T>`** wrapper — sempre checar `.Succeeded` antes de acessar `.Data`.
- **`System.Text.Json`** com `JsonSerializerDefaults.Web`.
- **API wrappers tipados**: `IOrdensServicoApi`, `IClientesApi` encapsulam `IApiClient`.
- **Form models** separados na pasta ViewModels.
- **Validacao client-side** antes da chamada API: `ValidarFormulario()` retorna `Dictionary<string, string[]>`.
- **`NormalizarCampo()`**: trim whitespace, converter vazio para null.

## Qualidade de Codigo

- **TreatWarningsAsErrors=true** — zero warnings. Corrigir, nao suprimir.
- **Nullable reference types** habilitados — tratar nullability explicitamente: `is not null`, `is null`, `?.`, `??`, `default!`.
- **Pattern matching**: `is StatusOS.Rascunho or StatusOS.Orcamento`, `exception is DomainException or ArgumentException`.
- **Expression-bodied members** para metodos/propriedades de uma linha.
- **Sem `this.`** exceto para desambiguacao.
- **Sem `#region`**.
- **`var`** quando o tipo e obvio pelo lado direito.
- **`string.IsNullOrWhiteSpace()`** sempre preferido sobre `string.IsNullOrEmpty()`.
- **Sem magic numbers/strings** — usar constantes, enums ou Value Objects.
- **Precisao decimal**: sempre `Math.Round(value, 2)` para calculos monetarios.

## Regras de Dependencia (ESTRITO)

- **Domain**: NAO referencia nada. Zero project references, zero NuGet.
- **Application**: referencia apenas Domain. NuGet: apenas FluentValidation.
- **Infrastructure**: referencia Domain e Application. NuGet: EF Core, Pomelo MySQL, StackExchange.Redis.
- **Api**: referencia Application e Infrastructure. NuGet: Swashbuckle.
- **Web**: NAO referencia Domain/Application/Infrastructure — comunica apenas via HTTP com a API.

## Performance

- **`.AsSplitQuery()`** em queries com multiplas colecoes.
- **`.AsNoTracking()`** em queries read-only.
- **Sealed classes** para devirtualizacao JIT.
- **Sem lazy loading** — todo Include deve ser explicito.
- **Redis caching** para dados lidos frequentemente via `ICacheService`.
- **Paginacao** em todos os endpoints de listagem — nunca retornar colecoes ilimitadas.

## Testes

- **xUnit** como framework, **Moq** para mocking (Application/Web).
- **Bogus** para geracao de dados em testes de integracao.
- **Domain tests**: puros, sem mock — testar comportamento da entidade via factory methods.
- **Application tests**: mock de repositorios e UnitOfWork, verificar orquestracao do service.
- **Web tests**: mock de interfaces de API service, testar transicoes de estado do ViewModel.
