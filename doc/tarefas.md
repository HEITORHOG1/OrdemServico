# ✅ Tarefas de Implementação — Ordem de Serviço API

> **Instrução:** Cada tarefa será implementada uma por vez. Marque com `[x]` quando concluída.
> Diga o número da tarefa para eu implementar.

---

## Fase 1 — Domain

### 1.1 Enums

- [x] **T-001** · Criar `StatusOS.cs` — Enum com os 8 estados da OS ✅
- [x] **T-002** · Criar `TipoDesconto.cs` — Enum (Percentual, ValorFixo) ✅
- [x] **T-003** · Criar `MeioPagamento.cs` — Enum (Dinheiro, PIX, CartaoCredito, CartaoDebito, Boleto, Transferencia) ✅

### 1.2 Exceções

- [x] **T-004** · Criar `DomainException.cs` — Exceção base de domínio ✅
- [x] **T-005** · Criar `StatusTransicaoInvalidaException.cs` — Transição de status não permitida ✅
- [x] **T-006** · Criar `DescontoExcedeTotalException.cs` — Desconto maior que o total ✅

### 1.3 Value Objects

- [x] **T-007** · Criar `Dinheiro.cs` — Valor monetário (decimal, 2 casas, ≥ 0, operadores +/-) ✅
- [x] **T-008** · Criar `NumeroOS.cs` — Número formatado OS-YYYYMMDD-XXXX ✅
- [x] **T-009** · Criar `Desconto.cs` — Encapsula tipo + valor, calcula desconto efetivo ✅

### 1.4 Entidades

- [x] **T-010** · Criar `Cliente.cs` — Entidade com factory method, validação de nome obrigatório ✅
- [x] **T-011** · Criar `Equipamento.cs` — Entidade vinculada a cliente (marca, modelo, aparelho, série) ✅
- [x] **T-012** · Criar `OrdemServicoServico.cs` — Item de serviço (descrição, qtd, valor unitário, subtotal) ✅
- [x] **T-013** · Criar `OrdemServicoProduto.cs` — Item de produto (descrição, qtd, valor unitário, subtotal) ✅
- [x] **T-014** · Criar `OrdemServicoTaxa.cs` — Taxa (descrição, valor) ✅
- [x] **T-015** · Criar `OrdemServicoPagamento.cs` — Pagamento (meio, valor, data) ✅
- [x] **T-016** · Criar `OrdemServicoFoto.cs` — Foto (url, legenda) ✅
- [x] **T-017** · Criar `OrdemServicoAnotacao.cs` — Anotação interna (texto, autor, data) ✅
- [x] **T-018** · Criar `OrdemServico.cs` — **Aggregate Root** com regras de transição de status, cálculo de total, gestão de coleções filhas ✅

### 1.5 Interfaces

- [x] **T-019** · Criar `IUnitOfWork.cs` — Gerenciador de transações ✅
- [x] **T-020** · Criar `IClienteRepository.cs` — Add, Update, GetById, ExistsByDocument ✅
- [x] **T-021** · Criar `IEquipamentoRepository.cs` — Add, Update, GetById, ListByClienteId ✅
- [x] **T-022** · Criar `IOrdemServicoRepository.cs` — Add, Update, GetById, ListPaginated, ObterProximoSequencial ✅
- [x] **T-023** · Criar `ICacheService.cs` — Contrato de cache (Get, Set, Remove) ✅

### 🔒 Gate — Build Domain

- [x] **T-024** · Executar `dotnet build src/Domain` — deve compilar com 0 erros e 0 pacotes NuGet ✅

---

## Fase 2 — Application

### 2.1 DTOs Comuns

- [x] **T-025** · Criar `PagedRequest.cs` — Record com Page e PageSize ✅
- [x] **T-026** · Criar `PagedResponse<T>.cs` — Record genérico com Items, TotalCount, TotalPages ✅

### 2.2 DTOs de Cliente

- [x] **T-027** · Criar `CriarClienteRequest.cs` ✅
- [x] **T-028** · Criar `ClienteResponse.cs` ✅

### 2.3 DTOs de Ordem de Serviço

- [x] **T-029** · Criar `CriarOrdemServicoRequest.cs` ✅
- [x] **T-030** · Criar `AtualizarOrdemServicoRequest.cs` ✅
- [x] **T-031** · Criar `OrdemServicoResponse.cs` — Response detalhado com itens ✅
- [x] **T-032** · Criar `OrdemServicoResumoResponse.cs` — Response resumido para listagem ✅
- [x] **T-033** · Criar `AdicionarServicoRequest.cs` ✅
- [x] **T-034** · Criar `AdicionarProdutoRequest.cs` ✅
- [x] **T-035** · Criar `AplicarDescontoRequest.cs` ✅
- [x] **T-036** · Criar `AdicionarTaxaRequest.cs` ✅
- [x] **T-037** · Criar `RegistrarPagamentoRequest.cs` ✅
- [x] **T-038** · Criar `AdicionarAnotacaoRequest.cs` ✅
- [x] **T-039** · Criar `AlterarStatusRequest.cs` ✅

### 2.4 Validators

- [x] **T-040** · Criar `CriarOrdemServicoValidator.cs` — ClienteId obrigatório, Defeito não vazio ✅
- [x] **T-041** · Criar `AtualizarOrdemServicoValidator.cs` ✅
- [x] **T-042** · Criar `AdicionarServicoValidator.cs` — Descrição não vazia, Qtd ≥ 1, Valor ≥ 0 ✅
- [x] **T-043** · Criar `AdicionarProdutoValidator.cs` — Descrição não vazia, Qtd ≥ 1, Valor ≥ 0 ✅
- [x] **T-044** · Criar `AplicarDescontoValidator.cs` — Tipo válido, Percentual ≤ 100 ✅
- [x] **T-045** · Criar `CriarClienteValidator.cs` — Nome obrigatório, Email válido ✅

### 2.5 Mappings

- [x] **T-046** · Criar `OrdemServicoMappings.cs` — Extensions ToResponse(), ToResumoResponse() ✅
- [x] **T-047** · Criar `ClienteMappings.cs` — Extensions ToResponse() ✅

### 2.6 Services

- [x] **T-048** · Criar `IOrdemServicoService.cs` — Interface dos casos de uso ✅
- [x] **T-049** · Criar `IClienteService.cs` — Interface CRUD ✅
- [x] **T-050** · Criar `IEquipamentoService.cs` — Interface CRUD ✅
- [x] **T-051** · Criar `OrdemServicoService.cs` — Implementação dos casos de uso ✅
- [x] **T-052** · Criar `ClienteService.cs` — Implementação CRUD ✅
- [x] **T-053** · Criar `EquipamentoService.cs` — Implementação CRUD ✅

### 2.7 DI da Camada

- [x] **T-054** · Criar `DependencyInjection.cs` (Application) — Registrar services + validators ✅

### 🔒 Gate — Build Application

- [x] **T-055** · Executar `dotnet build src/Application` — 0 erros. Único pacote: FluentValidation ✅

---

## Fase 3 — Infrastructure

### 3.1 DbContext

- [x] **T-056** · Adicionar pacote `Pomelo.EntityFrameworkCore.MySql` ao Infrastructure.csproj ✅
- [x] **T-057** · Criar `AppDbContext.cs` — DbSets + OnModelCreating ✅

### 3.2 Entity Configurations (Fluent API)

- [x] **T-058** · Criar `ClienteConfiguration.cs` ✅
- [x] **T-059** · Criar `EquipamentoConfiguration.cs` ✅
- [x] **T-060** · Criar `OrdemServicoConfiguration.cs` ✅
- [x] **T-061** · Criar `OrdemServicoServicoConfiguration.cs` ✅
- [x] **T-062** · Criar `OrdemServicoProdutoConfiguration.cs` ✅
- [x] **T-063** · Criar `OrdemServicoTaxaConfiguration.cs` ✅
- [x] **T-064** · Criar `OrdemServicoPagamentoConfiguration.cs` ✅
- [x] **T-065** · Criar `OrdemServicoFotoConfiguration.cs` ✅
- [x] **T-066** · Criar `OrdemServicoAnotacaoConfiguration.cs` ✅

### 3.3 UnitOfWork e Repositórios

- [x] **T-067** · Criar `UnitOfWork.cs` — Wraps SaveChangesAsync do DbContext ✅
- [x] **T-068** · Criar `OrdemServicoRepository.cs` — LINQ + Include + paginação ✅
- [x] **T-069** · Criar `ClienteRepository.cs` ✅
- [x] **T-070** · Criar `EquipamentoRepository.cs` ✅

### 3.4 Cache

- [x] **T-071** · Adicionar pacote `StackExchange.Redis` ao Infrastructure.csproj ✅
- [x] **T-072** · Criar `CacheKeys.cs` — Constantes de chaves ✅
- [x] **T-073** · Criar `RedisCacheService.cs` — Implementação de ICacheService ✅

### 3.5 Storage

- [x] **T-074** · Criar `AzureBlobStorageService.cs` — Upload de fotos (pode ser stub inicial) ✅

### 3.6 DI da Camada

- [x] **T-075** · Criar `DependencyInjection.cs` (Infrastructure) — Registrar DbContext, repos, cache, storage ✅

### 3.7 Migration

- [x] **T-076** · Gerar migration inicial: `dotnet ef migrations add InitialCreate` ⚠️ (Aviso)

### 🔒 Gate — Build Infrastructure

- [x] **T-077** · Executar `dotnet build src/Infrastructure` — 0 erros ✅

---

## Fase 4 — Api

### 4.1 Extensions

- [x] **T-078** · Criar `ServiceCollectionExtensions.cs` — Organiza DI no Program.cs ✅
- [x] **T-079** · Criar `WebApplicationExtensions.cs` — Organiza pipeline no Program.cs ✅

### 4.2 Middleware

- [x] **T-080** · Criar `GlobalExceptionHandler.cs` — DomainException → 422, NotFound → 404 ✅
- [x] **T-081** · Criar `RequestLoggingMiddleware.cs` — Log de request/response ✅
- [x] **T-082** · Criar `CorrelationIdMiddleware.cs` — Correlation ID no header ✅

### 4.3 Filters

- [x] **T-083** · Criar `ValidationFilter.cs` — Executa FluentValidation nos endpoints ✅

### 4.4 Endpoints

- [x] **T-084** · Criar `OrdemServicoEndpoints.cs` — Todas as rotas de OS ✅
- [x] **T-085** · Criar `ClienteEndpoints.cs` — Rotas de cliente ✅
- [x] **T-086** · Criar `EquipamentoEndpoints.cs` — Rotas de equipamento ✅

### 4.5 Composição Raiz

- [x] **T-087** · Configurar `Program.cs` — DI + Pipeline + Endpoints + Swagger ✅

### 4.6 Configuração

- [x] **T-088** · Configurar `appsettings.json` — Connection strings (MySQL, Redis) ✅
- [x] **T-089** · Configurar `appsettings.Development.json` — Settings de desenvolvimento ✅

### 🔒 Gate — API Rodando

- [x] **T-090** · Executar `dotnet run --project src/Api` — API sobe, Swagger acessível ✅

---

## Fase 5 — Testes

### 5.1 Domain Unit Tests

- [ ] **T-091** · Criar `OrdemServicoTests.cs` — Transições de status, regras de negócio
- [ ] **T-092** · Criar `OrdemServicoCalculoTests.cs` — Cálculos de total e desconto
- [ ] **T-093** · Criar `ClienteTests.cs` — Criação e validações
- [ ] **T-094** · Criar `DinheiroTests.cs` — Operações e edge cases
- [ ] **T-095** · Criar `NumeroOSTests.cs` — Formatação e parsing
- [ ] **T-096** · Criar `DescontoTests.cs` — Percentual vs fixo

### 5.2 Application Unit Tests

- [ ] **T-097** · Criar `OrdemServicoServiceTests.cs` — Casos de uso com mocks
- [ ] **T-098** · Criar `ClienteServiceTests.cs` — CRUD com mocks
- [ ] **T-099** · Criar `CriarOrdemServicoValidatorTests.cs` — Regras de validação

### 5.3 Integration Tests

- [ ] **T-100** · Criar `WebApplicationFixture.cs` — Setup TestServer + banco in-memory
- [ ] **T-101** · Criar `OrdemServicoEndpointsTests.cs` — Fluxo completo via HTTP
- [ ] **T-102** · Criar `ClienteEndpointsTests.cs` — CRUD via HTTP
- [ ] **T-103** · Criar `OrdemServicoTestData.cs` — Builders/Fakers com Bogus

### 🔒 Gate — Testes Passando

- [ ] **T-104** · Executar `dotnet test` — Todos os testes passando

---

## Progresso

| Fase | Tarefas | Concluídas | % |
|---|---|---|---|
| 1 · Domain | 24 | 0 | 0% |
| 2 · Application | 31 | 0 | 0% |
| 3 · Infrastructure | 22 | 0 | 0% |
| 4 · Api | 13 | 0 | 0% |
| 5 · Testes | 14 | 0 | 0% |
| **Total** | **104** | **0** | **0%** |
