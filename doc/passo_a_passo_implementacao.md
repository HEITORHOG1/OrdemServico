# 📚 Passo a Passo — Implementação Clean Architecture

> [!NOTE]
> Este documento é um **guia de estudo** que explica a ordem e o porquê de cada classe criada no projeto. Siga os passos na sequência — cada um depende do anterior.

---

## 🎯 Princípio Fundamental

No Clean Architecture, **sempre construímos de dentro para fora**:

```
                    ④ Api (Presentation)
                  ③ Infrastructure
                ② Application
              ① Domain ← COMEÇAMOS AQUI
```

**Por quê?** Porque as camadas externas dependem das internas. Se você construir de fora para dentro, vai precisar de coisas que ainda não existem. Construindo de dentro para fora, cada camada encontra tudo que precisa já pronto.

---

## Fase 1 — Domain (O Coração)

> **Objetivo:** Definir as entidades, regras de negócio, enums e contratos (interfaces). Aqui não existe banco de dados, HTTP, ou framework — apenas C# puro.

### Por que começar pelo Domain?

O Domain é a camada que **não depende de ninguém**. Ele define "o que o sistema É" — as regras de negócio que existiriam mesmo sem computador. Uma Ordem de Serviço tem status, tem cálculo de total, tem regras de transição — isso é Domain.

---

### Passo 1.1 — Enums de Domínio

📁 `src/Domain/Enums/`

**O que são:** Enums representam conjuntos fixos de valores que o domínio reconhece. Criamos eles primeiro porque as entidades vão usá-los.

| # | Arquivo | O que define |
|---|---|---|
| 1 | `StatusOS.cs` | Os 8 estados da OS: Rascunho, Orcamento, Aprovada, Rejeitada, EmAndamento, AguardandoPeca, Concluida, Entregue |
| 2 | `TipoDesconto.cs` | Tipo de desconto: Percentual ou ValorFixo |
| 3 | `MeioPagamento.cs` | Meios de pagamento: Dinheiro, PIX, CartaoCredito, CartaoDebito, Boleto, Transferencia |

**💡 O que você vai aprender:**
- Enums em C# com valores explícitos
- Por que definir enums no Domain e não em outras camadas

**Exemplo conceitual:**
```csharp
namespace Domain.Enums;

public enum StatusOS
{
    Rascunho = 0,
    Orcamento = 1,
    Aprovada = 2,
    Rejeitada = 3,
    EmAndamento = 4,
    AguardandoPeca = 5,
    Concluida = 6,
    Entregue = 7
}
```

> [!TIP]
> Sempre atribua valores inteiros explícitos aos enums. Isso evita problemas quando você reordenar os membros no futuro — o valor no banco de dados não muda.

---

### Passo 1.2 — Exceções de Domínio

📁 `src/Domain/Exceptions/`

**O que são:** Exceções tipadas que representam violações de regras de negócio. Criamos antes das entidades porque as entidades vão lançá-las.

| # | Arquivo | Quando é lançada |
|---|---|---|
| 4 | `DomainException.cs` | Classe base para todas as exceções de domínio |
| 5 | `StatusTransicaoInvalidaException.cs` | Quando tentam mudar o status da OS para um estado não permitido |
| 6 | `DescontoExcedeTotalException.cs` | Quando o desconto ultrapassa o valor total da OS |

**💡 O que você vai aprender:**
- Herança de exceções
- Por que criar exceções específicas em vez de usar `Exception` genérica
- Como a camada de Api vai converter essas exceções em HTTP status codes

**Exemplo conceitual:**
```csharp
namespace Domain.Exceptions;

// Classe base — todas as exceções de domínio herdam dela
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Exceção específica — carrega contexto sobre o erro
public sealed class StatusTransicaoInvalidaException : DomainException
{
    public StatusOS StatusAtual { get; }
    public StatusOS StatusDesejado { get; }

    public StatusTransicaoInvalidaException(StatusOS atual, StatusOS desejado)
        : base($"Transição de '{atual}' para '{desejado}' não é permitida.")
    {
        StatusAtual = atual;
        StatusDesejado = desejado;
    }
}
```

> [!IMPORTANT]
> **`DomainException` NÃO é `sealed`** — é a única classe no projeto que não é sealed, porque serve de base para outras exceções. Todas as exceções filhas **são `sealed`**.

---

### Passo 1.3 — Value Objects

📁 `src/Domain/ValueObjects/`

**O que são:** Objetos que representam conceitos do domínio por seu valor, não por identidade. Dois `Dinheiro(100.00)` são iguais, mesmo sendo instâncias diferentes. Diferente de uma Entidade, que tem um ID.

| # | Arquivo | O que representa |
|---|---|---|
| 7 | `Dinheiro.cs` | Valor monetário com precisão decimal (2 casas). Nunca negativo. |
| 8 | `NumeroOS.cs` | Número formatado da OS (ex: `OS-20260307-0001`). Imutável após criação. |
| 9 | `Desconto.cs` | Encapsula tipo (% ou fixo) + valor. Sabe calcular o valor efetivo. |

**💡 O que você vai aprender:**
- Diferença entre **Value Object** e **Entity**
- Imutabilidade (`record` ou `readonly struct`)
- Sobrescrita de `Equals` e `GetHashCode` (ou uso de `record`)
- Validação no construtor (fail fast)

**Exemplo conceitual:**
```csharp
namespace Domain.ValueObjects;

// record garante: imutabilidade, Equals por valor, GetHashCode automático
public sealed record Dinheiro
{
    public decimal Valor { get; }

    public Dinheiro(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor monetário não pode ser negativo.", nameof(valor));

        Valor = Math.Round(valor, 2);
    }

    // Operações ricas no Value Object
    public static Dinheiro operator +(Dinheiro a, Dinheiro b) => new(a.Valor + b.Valor);
    public static Dinheiro operator -(Dinheiro a, Dinheiro b) => new(a.Valor - b.Valor);
    public static Dinheiro Zero => new(0);
}
```

> [!TIP]
> **Por que usar `record`?** Em C#, `record` gera automaticamente `Equals`, `GetHashCode`, e `ToString` baseados nos valores, que é exatamente o comportamento de um Value Object.

---

### Passo 1.4 — Entidades de Domínio

📁 `src/Domain/Entities/`

**O que são:** Objetos com identidade única (ID) que encapsulam regras de negócio. São o coração do sistema. Diferente de DTOs, entidades **têm comportamento** — elas validam, calculam, e protegem invariantes.

**Ordem de criação importa!** Criamos primeiro as entidades que não dependem de outras:

| # | Arquivo | Descrição | Depende de |
|---|---|---|---|
| 10 | `Cliente.cs` | Dados do cliente (nome, documento, contato) | — |
| 11 | `Equipamento.cs` | Equipamento vinculado a um cliente | Cliente (FK) |
| 12 | `OrdemServicoServico.cs` | Item de serviço dentro da OS | — |
| 13 | `OrdemServicoProduto.cs` | Item de produto/peça dentro da OS | — |
| 14 | `OrdemServicoTaxa.cs` | Taxa aplicada na OS | — |
| 15 | `OrdemServicoPagamento.cs` | Registro de pagamento da OS | MeioPagamento (enum) |
| 16 | `OrdemServicoFoto.cs` | Foto anexada à OS | — |
| 17 | `OrdemServicoAnotacao.cs` | Anotação interna (não visível ao cliente) | — |
| 18 | `OrdemServico.cs` | **Aggregate Root** — entidade principal que orquestra tudo | Todas acima |

**💡 O que você vai aprender:**
- **Aggregate Root** — a entidade raiz que controla o acesso a suas filhas
- `private set` — propriedades que só a própria entidade pode alterar
- Métodos de domínio que encapsulam regras (ex: `AprovarOrcamento()`)
- Coleções `IReadOnlyCollection` para evitar manipulação externa
- Factory methods (`static Create(...)`) vs construtor

**Exemplo conceitual — Entidade simples:**
```csharp
namespace Domain.Entities;

public sealed class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string? Documento { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Endereco { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Construtor privado — força uso do factory method
    private Cliente() { } // Para EF Core

    public static Cliente Criar(string nome, string? documento, string? telefone, string? email, string? endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do cliente é obrigatório.", nameof(nome));

        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Documento = documento,
            Telefone = telefone,
            Email = email,
            Endereco = endereco,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Atualizar(string nome, string? documento, string? telefone, string? email, string? endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do cliente é obrigatório.", nameof(nome));

        Nome = nome;
        Documento = documento;
        Telefone = telefone;
        Email = email;
        Endereco = endereco;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Exemplo conceitual — Aggregate Root (OrdemServico):**
```csharp
namespace Domain.Entities;

public sealed class OrdemServico
{
    public Guid Id { get; private set; }
    public NumeroOS Numero { get; private set; }
    public StatusOS Status { get; private set; }

    // Coleções protegidas — só a entidade pode adicionar/remover
    private readonly List<OrdemServicoServico> _servicos = new();
    public IReadOnlyCollection<OrdemServicoServico> Servicos => _servicos.AsReadOnly();

    // Métodos de domínio encapsulam regras de negócio
    public void AdicionarServico(string descricao, int quantidade, decimal valorUnitario)
    {
        if (Status != StatusOS.Rascunho)
            throw new DomainException("Só é possível adicionar serviços quando a OS está em Rascunho.");

        _servicos.Add(OrdemServicoServico.Criar(Id, descricao, quantidade, valorUnitario, _servicos.Count + 1));
    }

    public void AprovarOrcamento()
    {
        if (Status != StatusOS.Orcamento)
            throw new StatusTransicaoInvalidaException(Status, StatusOS.Aprovada);

        Status = StatusOS.Aprovada;
        UpdatedAt = DateTime.UtcNow;
    }

    public Dinheiro CalcularTotal()
    {
        var subtotalServicos = _servicos.Sum(s => s.Subtotal);
        var subtotalProdutos = _produtos.Sum(p => p.Subtotal);
        var totalTaxas = _taxas.Sum(t => t.Valor);
        var descontoCalculado = Desconto?.CalcularValor(subtotalServicos + subtotalProdutos) ?? 0;

        return new Dinheiro(subtotalServicos + subtotalProdutos - descontoCalculado + totalTaxas);
    }
}
```

> [!IMPORTANT]
> **Conceito-chave: Aggregate Root.** `OrdemServico` é o "portão de entrada" para todas as suas entidades filhas. Você **nunca** acessa `OrdemServicoServico` diretamente pelo repositório — sempre passa pela `OrdemServico`. Isso garante que as regras de negócio são sempre respeitadas.

---

### Passo 1.5 — Interfaces (Contratos)

📁 `src/Domain/Interfaces/` e `src/Domain/Interfaces/Repositories/`

**O que são:** Contratos que definem O QUE o sistema precisa, sem definir COMO será feito. A camada de Infrastructure vai implementar esses contratos depois.

| # | Arquivo | O que define |
|---|---|---|
| 19 | `Repositories/IOrdemServicoRepository.cs` | Operações de persistência da OS |
| 20 | `Repositories/IClienteRepository.cs` | Operações de persistência de clientes |
| 21 | `Repositories/IEquipamentoRepository.cs` | Operações de persistência de equipamentos |
| 22 | `IUnitOfWork.cs` | Controle transacional (SaveChanges) |
| 23 | `ICacheService.cs` | Abstração de cache (get/set/remove) |

**💡 O que você vai aprender:**
- **Inversão de Dependência** (o "D" do SOLID) — Domain define o contrato, Infrastructure implementa
- Por que interfaces ficam no Domain e não na Infrastructure
- Design de repositórios: quais métodos expor

**Exemplo conceitual:**
```csharp
namespace Domain.Interfaces.Repositories;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OrdemServico?> GetByNumeroAsync(string numero, CancellationToken cancellationToken);
    Task<(IReadOnlyList<OrdemServico> Items, int TotalCount)> ListAsync(
        int page, int pageSize, StatusOS? status, CancellationToken cancellationToken);
    Task AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken);
    Task UpdateAsync(OrdemServico ordemServico, CancellationToken cancellationToken);
}
```

> [!TIP]
> **Por que a interface fica no Domain?** Porque é o Domain que PRECISA de persistência. A Infrastructure IMPLEMENTA. Se a interface ficasse na Infrastructure, o Domain dependeria dela — violando a regra de dependência.

---

### ✅ Checkpoint — Fim da Fase 1

Ao final desta fase, você terá:
- **3 enums** que definem os vocabulários do domínio
- **3 exceções** tipadas para erros de negócio
- **3 value objects** imutáveis e validados
- **9 entidades** com regras de negócio embutidas
- **5 interfaces** que definem contratos sem implementação
- **Total: 23 arquivos** — todos em C# puro, sem nenhum pacote NuGet

```
src/Domain/
├── Enums/
│   ├── StatusOS.cs            ✅ Passo 1.1
│   ├── TipoDesconto.cs        ✅ Passo 1.1
│   └── MeioPagamento.cs       ✅ Passo 1.1
├── Exceptions/
│   ├── DomainException.cs     ✅ Passo 1.2
│   ├── StatusTransicaoInvalidaException.cs  ✅ Passo 1.2
│   └── DescontoExcedeTotalException.cs      ✅ Passo 1.2
├── ValueObjects/
│   ├── Dinheiro.cs            ✅ Passo 1.3
│   ├── NumeroOS.cs            ✅ Passo 1.3
│   └── Desconto.cs            ✅ Passo 1.3
├── Entities/
│   ├── Cliente.cs             ✅ Passo 1.4
│   ├── Equipamento.cs         ✅ Passo 1.4
│   ├── OrdemServicoServico.cs ✅ Passo 1.4
│   ├── OrdemServicoProduto.cs ✅ Passo 1.4
│   ├── OrdemServicoTaxa.cs    ✅ Passo 1.4
│   ├── OrdemServicoPagamento.cs ✅ Passo 1.4
│   ├── OrdemServicoFoto.cs    ✅ Passo 1.4
│   ├── OrdemServicoAnotacao.cs ✅ Passo 1.4
│   └── OrdemServico.cs        ✅ Passo 1.4
└── Interfaces/
    ├── IUnitOfWork.cs          ✅ Passo 1.5
    ├── ICacheService.cs        ✅ Passo 1.5
    └── Repositories/
        ├── IOrdemServicoRepository.cs  ✅ Passo 1.5
        ├── IClienteRepository.cs       ✅ Passo 1.5
        └── IEquipamentoRepository.cs   ✅ Passo 1.5
```

> [!IMPORTANT]
> **Antes de avançar para a Fase 2**, rode `dotnet build src/Domain` e garanta 0 erros. O Domain **não pode depender de nenhum pacote NuGet** — se pedir, algo está errado.

---

## Fase 2 — Application (Casos de Uso)

> **Objetivo:** Criar os DTOs, validações, serviços de aplicação e mapeamentos. Esta camada orquestra os casos de uso — ela sabe "o que fazer", mas delega "como fazer" para Domain (regras) e Infrastructure (persistência).

### Por que Application vem depois de Domain?

Porque Application **depende** de Domain:
- Usa as entidades para executar regras
- Usa as interfaces de repositório para persistir
- Cria DTOs que refletem a estrutura das entidades

---

### Passo 2.1 — DTOs Comuns

📁 `src/Application/DTOs/Common/`

**O que são:** DTOs (Data Transfer Objects) genéricos usados em múltiplos endpoints. Não contêm lógica — são apenas "envelopes" de dados.

| # | Arquivo | O que define |
|---|---|---|
| 24 | `PagedRequest.cs` | Parâmetros de paginação (Page, PageSize) |
| 25 | `PagedResponse.cs` | Resposta paginada genérica (Items, TotalCount, TotalPages) |

**💡 O que você vai aprender:**
- DTOs como `record` (imutáveis por natureza)
- Generics em C# (`PagedResponse<T>`)

**Exemplo conceitual:**
```csharp
namespace Application.DTOs.Common;

public sealed record PagedRequest(int Page = 1, int PageSize = 20);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
```

---

### Passo 2.2 — DTOs de Request e Response

📁 `src/Application/DTOs/OrdemServico/` e `src/Application/DTOs/Cliente/`

**O que são:** DTOs específicos para cada operação da API. Requests representam o que o cliente envia, Responses representam o que a API retorna.

**Ordem de criação — DTOs de Cliente:**

| # | Arquivo | Direção |
|---|---|---|
| 26 | `CriarClienteRequest.cs` | Request (API → Application) |
| 27 | `ClienteResponse.cs` | Response (Application → API) |

**Ordem de criação — DTOs de Ordem de Serviço:**

| # | Arquivo | Direção |
|---|---|---|
| 28 | `CriarOrdemServicoRequest.cs` | Request |
| 29 | `AtualizarOrdemServicoRequest.cs` | Request |
| 30 | `OrdemServicoResponse.cs` | Response (detalhado, com todos os itens) |
| 31 | `OrdemServicoResumoResponse.cs` | Response (resumido, para listagens) |
| 32 | `AdicionarServicoRequest.cs` | Request |
| 33 | `AdicionarProdutoRequest.cs` | Request |
| 34 | `AplicarDescontoRequest.cs` | Request |
| 35 | `AdicionarTaxaRequest.cs` | Request |
| 36 | `RegistrarPagamentoRequest.cs` | Request |
| 37 | `AdicionarAnotacaoRequest.cs` | Request |
| 38 | `AlterarStatusRequest.cs` | Request |

**💡 O que você vai aprender:**
- Separação entre Request e Response DTOs
- Por que **nunca** expor entidades do Domain diretamente na API
- DTOs como `record` para imutabilidade
- Diferença entre DTO detalhado (para GET por ID) e resumido (para listagem)

**Exemplo conceitual:**
```csharp
namespace Application.DTOs.OrdemServico;

// Request — o que o cliente envia para criar uma OS
public sealed record CriarOrdemServicoRequest(
    Guid ClienteId,
    Guid? EquipamentoId,
    string Defeito,
    string? Duracao,
    string? Observacoes,
    string? Referencia,
    DateOnly? Validade,
    DateOnly? Prazo);

// Response — o que a API retorna
public sealed record OrdemServicoResponse(
    Guid Id,
    string Numero,
    string Status,
    Guid ClienteId,
    string ClienteNome,
    string Defeito,
    // ... demais campos
    decimal TotalGeral,
    List<ServicoResponse> Servicos,
    List<ProdutoResponse> Produtos);
```

> [!WARNING]
> **Request ≠ Entity.** O DTO de request não tem ID (é gerado pelo sistema), não tem Status (começa sempre como Rascunho), não tem CreatedAt (é automático). Só contém o que o cliente pode informar.

---

### Passo 2.3 — Validators (FluentValidation)

📁 `src/Application/Validators/`

**O que são:** Classes que validam os DTOs de request antes de chegarem ao serviço de aplicação. Usam FluentValidation para regras declarativas.

| # | Arquivo | O que valida |
|---|---|---|
| 39 | `CriarOrdemServicoValidator.cs` | ClienteId obrigatório, Defeito não vazio, Validade ≥ hoje |
| 40 | `AtualizarOrdemServicoValidator.cs` | Campos que podem ser atualizados |
| 41 | `AdicionarServicoValidator.cs` | Descrição não vazia, Quantidade ≥ 1, Valor ≥ 0 |
| 42 | `AdicionarProdutoValidator.cs` | Descrição não vazia, Quantidade ≥ 1, Valor ≥ 0 |
| 43 | `AplicarDescontoValidator.cs` | Tipo válido, Valor > 0, Percentual ≤ 100 |
| 44 | `CriarClienteValidator.cs` | Nome obrigatório, Email formato válido (se informado) |

**💡 O que você vai aprender:**
- FluentValidation: `RuleFor`, `NotEmpty`, `GreaterThan`, `When`
- Diferença entre **validação de input** (Application) e **regra de negócio** (Domain)
- Como validators são executados automaticamente via filtro na API

**Exemplo conceitual:**
```csharp
namespace Application.Validators;

public sealed class CriarOrdemServicoValidator : AbstractValidator<CriarOrdemServicoRequest>
{
    public CriarOrdemServicoValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("Cliente é obrigatório.");

        RuleFor(x => x.Defeito)
            .NotEmpty()
            .WithMessage("Descrição do defeito é obrigatória.");

        RuleFor(x => x.Validade)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.Validade.HasValue)
            .WithMessage("Validade deve ser hoje ou no futuro.");
    }
}
```

> [!IMPORTANT]
> **Validação de input ≠ Regra de negócio.**
> - **Validação (Application):** "O campo ClienteId não pode ser vazio" → formato dos dados
> - **Regra (Domain):** "Não pode aprovar uma OS que está em Rascunho" → lógica de negócio

---

### Passo 2.4 — Mappings (Extensões de Mapeamento)

📁 `src/Application/Mappings/`

**O que são:** Extension methods que convertem Entidades em DTOs e vice-versa. Fazemos **manual** (sem AutoMapper) para ter controle total e facilitar refactoring.

| # | Arquivo | O que converte |
|---|---|---|
| 45 | `OrdemServicoMappings.cs` | OrdemServico ↔ DTOs (Response, Resumo) |
| 46 | `ClienteMappings.cs` | Cliente ↔ DTOs |

**💡 O que você vai aprender:**
- Extension methods em C#
- Mapeamento manual: mais código, mais controle, zero mágica
- Por que NÃO usamos AutoMapper

**Exemplo conceitual:**
```csharp
namespace Application.Mappings;

public static class ClienteMappings
{
    public static ClienteResponse ToResponse(this Cliente cliente) => new(
        Id: cliente.Id,
        Nome: cliente.Nome,
        Documento: cliente.Documento,
        Telefone: cliente.Telefone,
        Email: cliente.Email);
}
```

---

### Passo 2.5 — Application Services (Interfaces)

📁 `src/Application/Services/`

**O que são:** Interfaces que definem os casos de uso do sistema. Cada método representa uma ação que o usuário pode fazer.

| # | Arquivo | O que define |
|---|---|---|
| 47 | `IOrdemServicoService.cs` | Todos os casos de uso de OS |
| 48 | `IClienteService.cs` | CRUD de clientes |
| 49 | `IEquipamentoService.cs` | CRUD de equipamentos |

**Exemplo conceitual:**
```csharp
namespace Application.Services;

public interface IOrdemServicoService
{
    Task<OrdemServicoResponse> CriarAsync(CriarOrdemServicoRequest request, CancellationToken ct);
    Task<OrdemServicoResponse?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<PagedResponse<OrdemServicoResumoResponse>> ListarAsync(PagedRequest paging, StatusOS? status, CancellationToken ct);
    Task<OrdemServicoResponse> AtualizarAsync(Guid id, AtualizarOrdemServicoRequest request, CancellationToken ct);
    Task AlterarStatusAsync(Guid id, AlterarStatusRequest request, CancellationToken ct);
    // ... demais métodos
}
```

---

### Passo 2.6 — Application Services (Implementação)

📁 `src/Application/Services/`

**O que são:** A implementação dos casos de uso. Aqui acontece a orquestração: validar input → carregar entidade → executar regra de domínio → persistir → retornar response.

| # | Arquivo | O que implementa |
|---|---|---|
| 50 | `OrdemServicoService.cs` | Todos os casos de uso de OS |
| 51 | `ClienteService.cs` | CRUD de clientes |
| 52 | `EquipamentoService.cs` | CRUD de equipamentos |

**💡 O que você vai aprender:**
- **Dependency Injection** — receber repositórios via construtor
- Padrão: Buscar → Validar → Executar → Salvar → Retornar
- `CancellationToken` passado em todas as chamadas async
- `sealed` em classes que não serão herdadas

**Exemplo conceitual:**
```csharp
namespace Application.Services;

public sealed class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public OrdemServicoService(
        IOrdemServicoRepository repository,
        IClienteRepository clienteRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<OrdemServicoResponse> CriarAsync(
        CriarOrdemServicoRequest request,
        CancellationToken ct)
    {
        // 1. Verificar se cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, ct)
            ?? throw new DomainException($"Cliente '{request.ClienteId}' não encontrado.");

        // 2. Criar entidade (regras de domínio executadas aqui)
        var os = OrdemServico.Criar(cliente.Id, request.Defeito, ...);

        // 3. Persistir
        await _repository.AddAsync(os, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Invalidar cache
        await _cache.RemoveAsync(CacheKeys.OrdensServico.Lista, ct);

        // 5. Retornar response
        return os.ToResponse();
    }
}
```

---

### Passo 2.7 — DependencyInjection.cs

📁 `src/Application/`

| # | Arquivo | O que faz |
|---|---|---|
| 53 | `DependencyInjection.cs` | Extension method que registra todos os serviços da camada Application no container de DI |

**Exemplo conceitual:**
```csharp
namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IEquipamentoService, EquipamentoService>();

        // Registra todos os validators do assembly
        services.AddValidatorsFromAssemblyContaining<CriarOrdemServicoValidator>();

        return services;
    }
}
```

---

### ✅ Checkpoint — Fim da Fase 2

Ao final desta fase, você terá:
- **13 DTOs** (requests + responses)
- **6 validators** com regras de input
- **2 arquivos de mapeamento** manual
- **6 services** (3 interfaces + 3 implementações)
- **1 DependencyInjection** para registro no container
- **Total: 30 arquivos** (acumulado: 53)

> [!IMPORTANT]
> **Antes de avançar para a Fase 3**, rode `dotnet build src/Application` e garanta 0 erros. Application depende **apenas** de Domain + FluentValidation.

---

## Fase 3 — Infrastructure (Implementação Técnica)

> **Objetivo:** Implementar os contratos definidos no Domain — repositórios com EF Core, cache com Redis, storage de arquivos. Aqui é onde o banco de dados, as queries e os detalhes técnicos vivem.

### Por que Infrastructure vem depois?

Porque Infrastructure **implementa** as interfaces criadas no Domain. Sem as interfaces, não há o que implementar.

---

### Passo 3.1 — AppDbContext (EF Core)

📁 `src/Infrastructure/Data/`

| # | Arquivo | O que faz |
|---|---|---|
| 54 | `AppDbContext.cs` | DbContext principal — define os DbSets e aplica as Configurations |

**💡 O que você vai aprender:**
- `DbContext` do EF Core
- `DbSet<T>` para cada entidade
- `OnModelCreating` para aplicar configurações Fluent API
- Connection string via DI

---

### Passo 3.2 — Entity Configurations (Fluent API)

📁 `src/Infrastructure/Data/Configurations/`

**O que são:** Classes que definem como cada entidade é mapeada para o banco MySQL — nome da tabela, tipos de coluna, índices, relacionamentos.

| # | Arquivo | O que configura |
|---|---|---|
| 55 | `ClienteConfiguration.cs` | Tabela `clientes`, colunas, índices |
| 56 | `EquipamentoConfiguration.cs` | Tabela `equipamentos`, FK para cliente |
| 57 | `OrdemServicoConfiguration.cs` | Tabela `ordens_servico`, todas as colunas e FKs |
| 58 | `OrdemServicoServicoConfiguration.cs` | Tabela `os_servicos` |
| 59 | `OrdemServicoProdutoConfiguration.cs` | Tabela `os_produtos` |
| 60 | `OrdemServicoTaxaConfiguration.cs` | Tabela `os_taxas` |
| 61 | `OrdemServicoPagamentoConfiguration.cs` | Tabela `os_pagamentos` |
| 62 | `OrdemServicoFotoConfiguration.cs` | Tabela `os_fotos` |
| 63 | `OrdemServicoAnotacaoConfiguration.cs` | Tabela `os_anotacoes` |

**💡 O que você vai aprender:**
- `IEntityTypeConfiguration<T>` — separação de responsabilidade
- Fluent API vs Data Annotations (e por que preferimos Fluent API)
- Configuração de Value Objects com `OwnsOne`
- Tipos de coluna para MySQL (CHAR(36), DECIMAL(18,2), LONGBLOB)

**Exemplo conceitual:**
```csharp
namespace Infrastructure.Data.Configurations;

public sealed class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("ordens_servico");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("char(36)");

        builder.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Numero).IsUnique();

        builder.Property(x => x.Status).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.Defeito).HasColumnType("text").IsRequired();

        // Value Object mapeado como Owned Type
        builder.OwnsOne(x => x.DescontoAplicado, d =>
        {
            d.Property(dd => dd.Tipo).HasColumnName("tipo_desconto").HasColumnType("tinyint");
            d.Property(dd => dd.Valor).HasColumnName("valor_desconto").HasColumnType("decimal(18,2)");
        });

        // Relacionamentos
        builder.HasMany(x => x.Servicos).WithOne().HasForeignKey("ordem_servico_id");
        builder.HasMany(x => x.Produtos).WithOne().HasForeignKey("ordem_servico_id");
    }
}
```

---

### Passo 3.3 — UnitOfWork

📁 `src/Infrastructure/Data/`

| # | Arquivo | O que faz |
|---|---|---|
| 64 | `UnitOfWork.cs` | Wraps `DbContext.SaveChangesAsync()`. Garante que todas as operações de um caso de uso são salvas em uma única transação. |

---

### Passo 3.4 — Repositórios

📁 `src/Infrastructure/Repositories/`

| # | Arquivo | O que implementa |
|---|---|---|
| 65 | `OrdemServicoRepository.cs` | `IOrdemServicoRepository` — queries com EF Core |
| 66 | `ClienteRepository.cs` | `IClienteRepository` |
| 67 | `EquipamentoRepository.cs` | `IEquipamentoRepository` |

**💡 O que você vai aprender:**
- Repositórios sobre `DbContext`
- LINQ com `.Include()`, `.Where()`, `.OrderBy()`
- Paginação com `.Skip()` e `.Take()`
- `AsSplitQuery()` para evitar explosão cartesiana
- `AsNoTracking()` para queries de leitura

---

### Passo 3.5 — Cache (Redis)

📁 `src/Infrastructure/Cache/`

| # | Arquivo | O que faz |
|---|---|---|
| 68 | `CacheKeys.cs` | Constantes com as chaves de cache usadas no sistema |
| 69 | `RedisCacheService.cs` | Implementação da `ICacheService` com StackExchange.Redis |

---

### Passo 3.6 — Storage

📁 `src/Infrastructure/Storage/`

| # | Arquivo | O que faz |
|---|---|---|
| 70 | `AzureBlobStorageService.cs` | Upload de fotos para Azure Blob Storage |

---

### Passo 3.7 — DependencyInjection.cs

📁 `src/Infrastructure/`

| # | Arquivo | O que faz |
|---|---|---|
| 71 | `DependencyInjection.cs` | Registra DbContext, repositórios, cache e storage no container de DI |

---

### Passo 3.8 — Migration Inicial

📁 `src/Infrastructure/Migrations/`

| # | Ação | O que faz |
|---|---|---|
| 72 | `dotnet ef migrations add InitialCreate` | Gera a migration automática a partir das Configurations |

> [!NOTE]
> As migrations são **geradas automaticamente** pelo EF Core a partir das Entity Configurations. Você não escreve SQL — o EF Core gera para MySQL.

---

### ✅ Checkpoint — Fim da Fase 3

Ao final desta fase, você terá:
- **1 DbContext** configurado para MySQL
- **9 Entity Configurations** (Fluent API)
- **1 UnitOfWork**
- **3 repositórios** implementados
- **2 arquivos de cache** (keys + service)
- **1 storage** para fotos
- **1 DependencyInjection**
- **1 migration** gerada
- **Total: 19 arquivos** (acumulado: 72)

> [!IMPORTANT]
> **Antes de avançar**, rode `dotnet build src/Infrastructure` e `dotnet ef migrations list` para garantir que tudo está correto.

---

## Fase 4 — Api (Apresentação)

> **Objetivo:** Criar endpoints HTTP, middleware, filtros e a composição raiz (Program.cs). Esta é a camada que o mundo externo enxerga.

### Por que Api é a última?

Porque Api **depende de tudo**: usa Application Services para executar casos de uso, e Infrastructure para registro de DI. Tudo precisa estar pronto antes.

---

### Passo 4.1 — Extensions (Organização do Program.cs)

📁 `src/Api/Extensions/`

| # | Arquivo | O que faz |
|---|---|---|
| 73 | `ServiceCollectionExtensions.cs` | Métodos de extensão para organizar registro de serviços |
| 74 | `WebApplicationExtensions.cs` | Métodos de extensão para organizar o pipeline de middleware |

---

### Passo 4.2 — Middleware

📁 `src/Api/Middleware/`

| # | Arquivo | O que faz |
|---|---|---|
| 75 | `GlobalExceptionHandler.cs` | Captura exceções e converte em `ProblemDetails` com HTTP status correto |
| 76 | `RequestLoggingMiddleware.cs` | Loga request/response com correlation ID |
| 77 | `CorrelationIdMiddleware.cs` | Gera/propaga ID de correlação para tracing |

**💡 O que você vai aprender:**
- `IExceptionHandler` (novo no .NET 8+)
- ProblemDetails (RFC 7807) para padronizar erros da API
- Middleware pipeline order
- Como `DomainException` → `422`, `NotFoundException` → `404`

---

### Passo 4.3 — Filters

📁 `src/Api/Filters/`

| # | Arquivo | O que faz |
|---|---|---|
| 78 | `ValidationFilter.cs` | Endpoint filter que executa FluentValidation automaticamente antes do handler |

---

### Passo 4.4 — Endpoints

📁 `src/Api/Endpoints/`

| # | Arquivo | O que faz |
|---|---|---|
| 79 | `OrdemServicoEndpoints.cs` | Mapeia todas as rotas de OS (≈15 endpoints) |
| 80 | `ClienteEndpoints.cs` | Mapeia rotas de cliente (4 endpoints) |
| 81 | `EquipamentoEndpoints.cs` | Mapeia rotas de equipamento (2 endpoints) |

**💡 O que você vai aprender:**
- Minimal APIs: `MapGroup`, `MapPost`, `MapGet`, `MapPut`, `MapPatch`, `MapDelete`
- Route groups com tags para Swagger
- Injeção de serviços diretamente nos handlers
- Retornos tipados: `Results.Ok()`, `Results.NotFound()`, `Results.Created()`

---

### Passo 4.5 — Program.cs (Composição Raiz)

📁 `src/Api/`

| # | Arquivo | O que faz |
|---|---|---|
| 82 | `Program.cs` | A composição raiz — registra DI, configura pipeline, mapeia endpoints |

**Exemplo conceitual:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// DI — cada camada se registra via extension method
builder.Services.AddApplication();       // Application layer
builder.Services.AddInfrastructure(      // Infrastructure layer
    builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline (ordem importa!)
app.UseCorrelationId();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Endpoints
app.MapOrdemServicoEndpoints();
app.MapClienteEndpoints();
app.MapEquipamentoEndpoints();

app.Run();
```

> [!TIP]
> **Composição Raiz** é o único lugar onde todas as camadas se encontram. É aqui que `IOrdemServicoRepository` é ligado a `OrdemServicoRepository`. Nenhuma outra parte do código sabe da implementação concreta.

---

### ✅ Checkpoint — Fim da Fase 4

Ao final desta fase, você terá:
- **2 extensions** para organização
- **3 middleware** para tratamento global
- **1 filter** para validação
- **3 endpoints** com todas as rotas
- **1 Program.cs** configurado
- **Total: 10 arquivos** (acumulado: 82)

> [!IMPORTANT]
> **Teste final:** `dotnet run --project src/Api` — a API deve subir, Swagger acessível, e endpoints respondendo.

---

## Fase 5 — Testes

> **Objetivo:** Garantir que tudo funciona. Testes são escritos **junto** com o código (ou logo depois), mas estão listados aqui como fase separada por organização didática.

### Passo 5.1 — Testes de Domain

📁 `tests/Domain.UnitTests/`

| # | Arquivo | O que testa |
|---|---|---|
| 83 | `Entities/OrdemServicoTests.cs` | Transições de status, adição de itens, regras de negócio |
| 84 | `Entities/OrdemServicoCalculoTests.cs` | Cálculo de total, desconto, taxas |
| 85 | `Entities/ClienteTests.cs` | Criação e atualização de cliente |
| 86 | `ValueObjects/DinheiroTests.cs` | Operações, validações, igualdade |
| 87 | `ValueObjects/NumeroOSTests.cs` | Formatação, parsing |
| 88 | `ValueObjects/DescontoTests.cs` | Cálculo percentual vs fixo |

### Passo 5.2 — Testes de Application

📁 `tests/Application.UnitTests/`

| # | Arquivo | O que testa |
|---|---|---|
| 89 | `Services/OrdemServicoServiceTests.cs` | Casos de uso com repositórios mockados |
| 90 | `Services/ClienteServiceTests.cs` | CRUD com mocks |
| 91 | `Validators/CriarOrdemServicoValidatorTests.cs` | Regras de validação de input |

### Passo 5.3 — Testes de Integração

📁 `tests/Api.IntegrationTests/`

| # | Arquivo | O que testa |
|---|---|---|
| 92 | `Fixtures/WebApplicationFixture.cs` | Setup do TestServer com banco em memória |
| 93 | `Endpoints/OrdemServicoEndpointsTests.cs` | Fluxo completo HTTP: criar, listar, alterar status |
| 94 | `Endpoints/ClienteEndpointsTests.cs` | CRUD de clientes via HTTP |
| 95 | `TestData/OrdemServicoTestData.cs` | Builders/Fakers com Bogus |

---

### ✅ Checkpoint — Fim da Fase 5

Total de testes: **13 arquivos** (acumulado: **95 arquivos** no projeto)

---

## 📊 Resumo Geral

| Fase | Camada | Arquivos | Pacotes NuGet |
|---|---|---|---|
| **1** | Domain | 23 | Nenhum |
| **2** | Application | 30 | FluentValidation.DependencyInjectionExtensions |
| **3** | Infrastructure | 19 | Pomelo.EntityFrameworkCore.MySql, StackExchange.Redis, Azure.Storage.Blobs |
| **4** | Api | 10 | Swashbuckle.AspNetCore |
| **5** | Tests | 13 | NSubstitute, Bogus, Microsoft.AspNetCore.Mvc.Testing |
| | **Total** | **95** | |

---

## 🗺️ Ordem Completa de Criação

Para referência rápida, a lista completa em ordem:

```
 1-3.   Enums (StatusOS, TipoDesconto, MeioPagamento)
 4-6.   Exceptions (DomainException, StatusTransicao, DescontoExcedeTotal)
 7-9.   Value Objects (Dinheiro, NumeroOS, Desconto)
10-18.  Entities (Cliente → Equipamento → Itens da OS → OrdemServico)
19-23.  Interfaces (Repositórios, UnitOfWork, CacheService)
24-25.  DTOs Common (PagedRequest, PagedResponse)
26-38.  DTOs específicos (Requests + Responses)
39-44.  Validators (FluentValidation)
45-46.  Mappings (Extension Methods)
47-52.  Services (Interfaces + Implementações)
53.     Application DI
54.     AppDbContext
55-63.  Entity Configurations
64.     UnitOfWork
65-67.  Repositories
68-69.  Cache (Keys + Redis)
70.     Storage (Blob)
71.     Infrastructure DI
72.     Migration Inicial
73-74.  Extensions (Api)
75-77.  Middleware
78.     Validation Filter
79-81.  Endpoints
82.     Program.cs
83-95.  Testes
```

> [!TIP]
> **Dica de estudo:** A cada passo, faça `dotnet build` na camada que está trabalhando. Se compilar sem erros, avance. Se der erro, provavelmente você pulou algum passo ou tem uma dependência circular (que nunca deveria existir no Clean Architecture).
