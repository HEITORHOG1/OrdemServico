# Fase 1 — Implementacao MVP SaaS

**Objetivo**: Transformar o OrdemServico em um SaaS multi-tenant vendavel.
**Prazo estimado**: 6-8 semanas
**Pre-requisito**: Ler este documento inteiro antes de iniciar qualquer task.

---

## Visao Geral das Features

| # | Feature | Semanas | Dependencias |
|---|---------|---------|-------------|
| F0 | Logging Estruturado (Serilog) | 0.5 | Nenhuma |
| F1 | Autenticacao (Identity + JWT) | 2 | F0 |
| F2 | Multi-Tenancy | 2 | F1 |
| F3 | Usuarios, Roles e Super Admin | 1-2 | F1, F2 |
| F4 | Audit Trail (Log de Atividades) | 0.5 | F1, F2 |
| F5 | Geracao de PDF | 1 | F2 |
| F6 | Notificacoes por Email | 1 | F2, F3 |
| F7 | Dashboard com KPIs | 1-2 | F2 |
| F8 | Onboarding + Billing | 1 | F1, F2, F3 |

**Ordem de execucao**: F0 → F1 → F2 → F3 → F4 → (F5, F6, F7 em paralelo) → F8

---

## Progresso Geral

- [ ] **F0** — Logging Estruturado (Serilog) — 8 tasks
- [ ] **F1** — Autenticacao (Identity + JWT) — 9 tasks
- [ ] **F2** — Multi-Tenancy — 9 tasks
- [ ] **F3** — Usuarios, Roles e Super Admin — 8 tasks
- [ ] **F4** — Audit Trail — 3 tasks
- [ ] **F5** — Geracao de PDF — 4 tasks
- [ ] **F6** — Notificacoes por Email — 3 tasks
- [ ] **F7** — Dashboard com KPIs — 3 tasks
- [ ] **F8** — Onboarding + Billing — 5 tasks

> **Total: 0/52 tasks concluidas**

---

## CONCEITOS FUNDAMENTAIS

### Dois niveis de administracao

O sistema tem **dois niveis completamente diferentes** de administracao:

| Conceito | Super Admin | Admin do Tenant |
|----------|-------------|-----------------|
| **Quem e** | Voce, dono da plataforma SaaS | O dono da empresa-cliente que contratou |
| **Escopo** | Todos os tenants, todo o sistema | Apenas seu proprio tenant |
| **TenantId** | NULL (nao pertence a nenhum tenant) | Tem TenantId fixo |
| **Pode fazer** | CRUD tenants, suspender contas, ver metricas globais, impersonar | CRUD usuarios do seu tenant, ver OS, configurar empresa |
| **Acessa via** | Painel administrativo `/admin` | Painel normal `/` |
| **Quantidade** | 1-3 pessoas (voce + cofundadores) | 1+ por tenant |

### Hierarquia completa de cargos

```
SuperAdmin (dono da plataforma)
  └── Admin (dono da empresa-cliente)
        ├── Gerente
        ├── Tecnico
        └── Atendente
```

---

## F0 — LOGGING ESTRUTURADO (Serilog)

### Contexto

Hoje o projeto usa o logger padrao do ASP.NET (`ILogger<T>`) com `RequestLoggingMiddleware` manual. Para SaaS profissional, precisamos de logging estruturado com contexto rico (TenantId, UsuarioId, CorrelationId) em todos os logs, sinks configuraveis (Console, Seq, arquivo, Datadog) e request logging automatico.

Serilog e a lib padrao do ecossistema .NET para isso. Deve ser a **primeira feature implementada** porque todas as outras dependem de logging correto para debug e monitoramento.

### Decisao Arquitetural

- **Serilog** como provider de logging (substitui o provider padrao do ASP.NET)
- **Enrichers automaticos**: TenantId, UsuarioId, CorrelationId, MachineName, Environment
- **Request logging** via `app.UseSerilogRequestLogging()` (substitui o `RequestLoggingMiddleware` manual)
- **Sinks**: Console (dev), File (dev/staging), Seq (staging/prod), ou qualquer sink futuro
- **Configuracao via `appsettings.json`** (nao hardcoded)
- **Nao criar dependencia de Serilog no Domain ou Application** — apenas Infrastructure e Api

### Tasks

- [ ] **F0-T1**: Pacotes NuGet

**Api.csproj**:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="9.*" />
```

**Infrastructure.csproj** (se precisar logar dentro de services de infra):

```xml
<PackageReference Include="Serilog.Sinks.Seq" Version="8.*" />
<PackageReference Include="Serilog.Sinks.File" Version="6.*" />
```

**Domain e Application**: NENHUM pacote Serilog. Continuam usando `ILogger<T>` do `Microsoft.Extensions.Logging` que o Serilog intercepta automaticamente via provider.

---

- [ ] **F0-T2**: Configuracao do Serilog no Program.cs da API

**Arquivo modificado**: `src/Api/Program.cs`

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    builder.Host.UseSerilog();

    // ... resto do builder

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TenantId", httpContext.User?.FindFirst("tenant_id")?.Value ?? "anonymous");
            diagnosticContext.Set("UsuarioId", httpContext.User?.FindFirst("usuario_id")?.Value ?? "anonymous");
            diagnosticContext.Set("CorrelationId", httpContext.Response.Headers["X-Correlation-Id"].ToString());
        };
    });

    // ... resto do pipeline

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicacao encerrou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
```

---

- [ ] **F0-T3**: Configuracao no appsettings.json

**Arquivo modificado**: `src/Api/appsettings.json`

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/ordemservico-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{TenantId}] [{UsuarioId}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithEnvironmentName"]
  }
}
```

**appsettings.Production.json** (futuro):

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    },
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://seq:5341" }
      }
    ]
  }
}
```

---

- [ ] **F0-T4**: Remover RequestLoggingMiddleware manual

**Arquivo a remover**: `src/Api/Middlewares/RequestLoggingMiddleware.cs`

O `app.UseSerilogRequestLogging()` substitui completamente o middleware manual com vantagens:

- Log automatico de request/response com duracao
- Enriquecimento com TenantId, UsuarioId, CorrelationId
- Nao loga health checks e static files (configuravel)
- Performance melhor (nao precisa do Stopwatch manual)

**Arquivo modificado**: `src/Api/Extensions/WebApplicationExtensions.cs`

Remover a linha:

```csharp
// REMOVER: app.UseMiddleware<RequestLoggingMiddleware>();
```

O `CorrelationIdMiddleware` continua — ele gera o header. O Serilog apenas le e enriquece.

---

- [ ] **F0-T5**: Enricher customizado de TenantId/UsuarioId

**Arquivo novo**: `src/Infrastructure/Logging/TenantLogEnricher.cs`

```csharp
using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging;

public sealed class TenantLogEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantLogEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        var tenantId = httpContext.User?.FindFirst("tenant_id")?.Value ?? "system";
        var usuarioId = httpContext.User?.FindFirst("usuario_id")?.Value ?? "anonymous";

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", tenantId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UsuarioId", usuarioId));
    }
}
```

---

- [ ] **F0-T6**: Logging estruturado nos Services existentes

Revisar os services para usar logging com propriedades estruturadas em vez de interpolacao:

```csharp
// ERRADO (interpolacao — perde estrutura):
_logger.LogInformation($"OS {os.Numero} criada para cliente {os.ClienteId}");

// CORRETO (structured logging — indexavel):
_logger.LogInformation("OS {NumeroOS} criada para cliente {ClienteId}", os.Numero, os.ClienteId);
```

Adicionar logs em pontos criticos:

- **OrdemServicoService**: log ao criar OS, alterar status, registrar pagamento
- **ClienteService**: log ao criar cliente com documento duplicado
- **AuthService** (futuro F1): log ao login, login falho, refresh token, tentativa de acesso inativo
- **GlobalExceptionHandler**: ja loga excecoes (manter, Serilog captura automaticamente)

---

- [ ] **F0-T7**: Serilog no Web (Blazor)

**Arquivo modificado**: `src/Web/Program.cs`

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
```

**Pacote NuGet no Web.csproj**:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="9.*" />
```

O Blazor Server tambem se beneficia do Serilog: exceptions em componentes, circuit disconnections, rendering errors — tudo fica estruturado e com contexto.

---

- [ ] **F0-T8**: Docker Compose — Seq (opcional, recomendado)

**Arquivo modificado**: `docker-compose.yml`

Adicionar servico Seq para visualizacao de logs em staging/dev:

```yaml
  # Seq (Log viewer — opcional)
  seq:
    image: datalust/seq:latest
    container_name: os_seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:5341"   # Ingestion
      - "8083:80"     # UI
    volumes:
      - seq_data:/data
    networks:
      - os_network
    profiles:
      - monitoring
```

Adicionar `seq_data` em volumes. Rodar com `docker-compose --profile monitoring up`.

Seq permite: buscar logs por TenantId, UsuarioId, CorrelationId, filtrar por nivel, ver dashboards de erros.

---

### Resumo F0

| Item | Detalhe |
|------|---------|
| Pacotes | `Serilog.AspNetCore`, `Serilog.Sinks.File`, `Serilog.Sinks.Seq` |
| Arquivos novos | 1 (TenantLogEnricher) |
| Arquivos modificados | 4 (Program.cs API, Program.cs Web, appsettings.json, WebApplicationExtensions) |
| Arquivos removidos | 1 (RequestLoggingMiddleware) |
| Impacto no Domain | Nenhum |
| Impacto no Application | Nenhum (continua usando ILogger<T>) |

---

## F1 — AUTENTICACAO (Identity + JWT)

### Contexto

Hoje o sistema usa apenas um header `X-Api-Key` compartilhado. Para SaaS, cada usuario precisa de login individual com JWT Bearer Token.

### Decisao Arquitetural
- **ASP.NET Identity** para gestao de usuarios (tabelas AspNetUsers, AspNetRoles, etc.)
- **JWT Bearer** para autenticacao na API
- **Refresh Token** para renovacao sem re-login
- O Identity fica na **Infrastructure** (e uma dependencia de infra, nao de dominio)
- A **Web (Blazor)** autentica via cookie (AuthenticationStateProvider do Blazor Server)
- A **API** autentica via JWT Bearer

### Tasks

- [ ] **F1-T1**: Entidade Usuario no Domain
**Camada**: Domain
**Arquivo**: `src/Domain/Entities/Usuario.cs`

Criar entidade `Usuario` que representa o usuario de negocio (separado do IdentityUser que e infra).

```
Propriedades:
- Id: Guid
- IdentityUserId: string (FK para AspNetUsers.Id)
- TenantId: Guid? (NULLABLE — Super Admin tem TenantId null)
- Nome: string (required)
- Email: string (required)
- Cargo: CargoUsuario enum
- Ativo: bool
- UltimoAcesso: DateTime?
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

**Regras**:
- Classe `sealed`, construtor privado, factory method `Criar()`
- Interface `IUsuarioRepository` em `Domain/Interfaces/`
- `private set` em todas as propriedades
- Metodo `RegistrarAcesso()` para atualizar `UltimoAcesso`
- Metodo `AlterarCargo(CargoUsuario novoCargo)` com validacao (nao pode rebaixar SuperAdmin)
- Metodo `Desativar()` e `Reativar()` (SuperAdmin nao pode ser desativado)
- Metodo `EhSuperAdmin() => Cargo == CargoUsuario.SuperAdmin`
- Metodo `PertenceAoTenant(Guid tenantId) => TenantId == tenantId`

**Guard clause no Criar()**: Se cargo e `SuperAdmin`, TenantId DEVE ser null. Se qualquer outro cargo, TenantId DEVE ter valor.

---

- [ ] **F1-T2**: Enum CargoUsuario
**Camada**: Domain
**Arquivo**: `src/Domain/Enums/CargoUsuario.cs`

```csharp
namespace Domain.Enums;

public enum CargoUsuario
{
    SuperAdmin = 0,   // Dono da plataforma SaaS — sem TenantId
    Admin = 1,        // Dono da empresa-cliente
    Gerente = 2,      // Gerente da empresa
    Tecnico = 3,      // Tecnico de campo
    Atendente = 4     // Atendimento ao publico
}
```

**IMPORTANTE**: `SuperAdmin` e o valor 0 para ser o mais privilegiado. A logica de autorizacao usa `cargo <= CargoUsuario.Admin` para checar se e admin ou superior.

---

- [ ] **F1-T3**: Configuracao do ASP.NET Identity
**Camada**: Infrastructure
**Arquivos novos**:
- `src/Infrastructure/Identity/AppIdentityUser.cs`
- `src/Infrastructure/Identity/IdentityConfiguration.cs`

**AppIdentityUser**: Herda de `IdentityUser`. Adiciona:
- `TenantId` (Guid?) — null para SuperAdmin
- `UsuarioId` (Guid, FK para Domain.Usuario)

**No AppDbContext**: Herdar de `IdentityDbContext<AppIdentityUser>` em vez de `DbContext`.

```
Antes:  public class AppDbContext : DbContext
Depois: public class AppDbContext : IdentityDbContext<AppIdentityUser>
```

**Migration**: Gerar migration `AddIdentityTables` com as tabelas do Identity.

**Pacotes NuGet necessarios (Infrastructure)**:
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

**Pacotes NuGet necessarios (Api)**:
- `Microsoft.AspNetCore.Authentication.JwtBearer`

---

- [ ] **F1-T4**: Servico de Autenticacao na Application
**Camada**: Application
**Arquivos novos**:
- `src/Application/Interfaces/IAuthService.cs`
- `src/Application/Services/AuthService.cs`
- `src/Application/DTOs/Auth/LoginRequest.cs`
- `src/Application/DTOs/Auth/LoginResponse.cs`
- `src/Application/DTOs/Auth/RegistrarUsuarioRequest.cs`
- `src/Application/DTOs/Auth/RefreshTokenRequest.cs`
- `src/Application/DTOs/Auth/AlterarSenhaRequest.cs`
- `src/Application/DTOs/Auth/EsqueciSenhaRequest.cs`
- `src/Application/DTOs/Auth/RedefinirSenhaRequest.cs`
- `src/Application/DTOs/Auth/Validators/LoginValidator.cs`
- `src/Application/DTOs/Auth/Validators/RegistrarUsuarioValidator.cs`
- `src/Application/DTOs/Auth/Validators/AlterarSenhaValidator.cs`
- `src/Application/DTOs/Auth/Validators/RedefinirSenhaValidator.cs`

**Interface IAuthService**:
```csharp
Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
Task RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken ct = default);
Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaRequest request, CancellationToken ct = default);
Task RevogarTokenAsync(Guid usuarioId, CancellationToken ct = default);
Task EsqueciSenhaAsync(EsqueciSenhaRequest request, CancellationToken ct = default);
Task RedefinirSenhaAsync(RedefinirSenhaRequest request, CancellationToken ct = default);
```

**LoginResponse**: `record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UsuarioResponse Usuario)`

**LoginRequest**: `record LoginRequest(string Email, string Senha)`

**DTOs de request**: todos como `record` positional.
**Validators**: FluentValidation para email, senha (min 8 chars, 1 maiuscula, 1 numero, 1 especial).

**Fluxo do LoginAsync**:
1. Buscar IdentityUser por email
2. Validar senha via UserManager
3. Buscar Usuario (domain) pelo IdentityUserId
4. Verificar se `Ativo == true`
5. Verificar se Tenant esta ativo (se nao for SuperAdmin)
6. Gerar JWT com claims: `sub`, `email`, `tenant_id` (null para SuperAdmin), `role`, `usuario_id`
7. Gerar RefreshToken
8. Chamar `usuario.RegistrarAcesso()`
9. Retornar LoginResponse

**Fluxo do EsqueciSenhaAsync**:
1. Buscar IdentityUser por email
2. Gerar token de reset via `UserManager.GeneratePasswordResetTokenAsync()`
3. Enviar email com link de reset (usa IEmailService da F6)
4. Nao revelar se o email existe ou nao (seguranca)

---

- [ ] **F1-T5**: Geracao de JWT na Infrastructure
**Camada**: Infrastructure
**Arquivos novos**:
- `src/Infrastructure/Identity/JwtTokenService.cs`
- `src/Infrastructure/Identity/IJwtTokenService.cs`
- `src/Infrastructure/Identity/JwtOptions.cs`
- `src/Infrastructure/Identity/RefreshToken.cs` (entidade para persistir refresh tokens)

**JwtOptions** (configuration — `appsettings.json` secao `Jwt`):
```
- SecretKey: string (min 32 chars)
- Issuer: string
- Audience: string
- ExpiracaoMinutos: int (default: 60)
- RefreshExpiracaoDias: int (default: 7)
```

**Claims no JWT**:
```csharp
new Claim("sub", usuario.Id.ToString()),
new Claim("email", usuario.Email),
new Claim("tenant_id", usuario.TenantId?.ToString() ?? ""),
new Claim("role", usuario.Cargo.ToString()),
new Claim("usuario_id", usuario.Id.ToString()),
new Claim("is_super_admin", usuario.EhSuperAdmin().ToString())
```

**RefreshToken** (tabela no banco):
```
- Id: Guid
- UsuarioId: Guid
- Token: string (SHA256 hash, nao plain text)
- ExpiresAt: DateTime
- CreatedAt: DateTime
- RevokedAt: DateTime?
- ReplacedByToken: string? (rotacao de tokens)
```

**Rotacao de Refresh Token**: Ao usar um refresh token, ele e revogado e um novo e emitido. Se um token revogado for reutilizado, revogar TODA a cadeia (indicativo de roubo).

---

- [ ] **F1-T6**: Endpoints de Auth na API
**Camada**: Api
**Arquivo novo**: `src/Api/Endpoints/AuthEndpoints.cs`

```
POST /api/auth/login             → LoginAsync                [publico]
POST /api/auth/refresh           → RefreshTokenAsync         [publico]
POST /api/auth/registrar         → RegistrarAsync            [Admin ou SuperAdmin]
POST /api/auth/alterar-senha     → AlterarSenhaAsync         [autenticado]
POST /api/auth/revogar           → RevogarTokenAsync         [autenticado]
POST /api/auth/esqueci-senha     → EsqueciSenhaAsync         [publico]
POST /api/auth/redefinir-senha   → RedefinirSenhaAsync       [publico, com token]
```

**Configuracao no pipeline**:
- Adicionar `builder.Services.AddAuthentication().AddJwtBearer(...)` em `ServiceCollectionExtensions`
- Adicionar `app.UseAuthentication()` e `app.UseAuthorization()` em `WebApplicationExtensions`
- Manter o `ApiKeyAuthMiddleware` como fallback para integracao de terceiros
- Rotas publicas: `/api/auth/login`, `/api/auth/refresh`, `/api/auth/esqueci-senha`, `/api/auth/redefinir-senha`, `/api/onboarding/signup`, `/swagger`, `/health`
- Todas as outras rotas exigem `[Authorize]`

---

- [ ] **F1-T7**: Auth no Blazor Web
**Camada**: Web
**Arquivos novos**:
- `src/Web/Services/Auth/IAuthApi.cs`
- `src/Web/Services/Auth/AuthApi.cs`
- `src/Web/Services/Auth/AuthStateProvider.cs`
- `src/Web/Services/Auth/TokenStorage.cs`
- `src/Web/Services/Auth/AuthDelegatingHandler.cs`
- `src/Web/Pages/Auth/LoginPage.razor`
- `src/Web/Pages/Auth/EsqueciSenhaPage.razor`
- `src/Web/Pages/Auth/RedefinirSenhaPage.razor`
- `src/Web/ViewModels/Auth/LoginViewModel.cs`
- `src/Web/ViewModels/Auth/EsqueciSenhaViewModel.cs`

**AuthStateProvider**: Herda de `AuthenticationStateProvider`. Armazena JWT em `ProtectedSessionStorage`. Notifica o Blazor quando o estado de autenticacao muda.

**AuthDelegatingHandler**: `DelegatingHandler` que injeta o header `Authorization: Bearer {token}` em toda request do HttpClient. Tenta refresh automatico se receber 401.

**LoginPage**: Formulario de login com email/senha. Redireciona para `/` apos sucesso. Link "Esqueci minha senha".

**Redirect nao autenticado**: `App.razor` deve usar `<CascadingAuthenticationState>` e `<AuthorizeRouteView>` para redirecionar para `/login` se nao autenticado.

**Esconder menus por cargo**: `NavMenu.razor` usa `<AuthorizeView Roles="Admin,SuperAdmin">` para mostrar/esconder itens.

---

- [ ] **F1-T8**: Seed do Super Admin
**Camada**: Infrastructure
**Arquivo modificado**: `src/Infrastructure/Persistence/DatabaseSeeder.cs`

No `SeedAsync()`, criar o primeiro SuperAdmin se nao existir:
1. Criar role "SuperAdmin" no Identity (se nao existir)
2. Criar roles "Admin", "Gerente", "Tecnico", "Atendente"
3. Criar IdentityUser com email do env var `SUPERADMIN_EMAIL` (default: `admin@ordemservico.com`)
4. Criar senha do env var `SUPERADMIN_PASSWORD` (default: `Admin@123456`)
5. Criar entidade `Usuario` com `CargoUsuario.SuperAdmin` e `TenantId = null`
6. Atribuir role "SuperAdmin"

**IMPORTANTE**: Em producao, o email/senha devem vir de variaveis de ambiente ou secrets, NUNCA hardcoded.

---

- [ ] **F1-T9**: Testes de Auth
**Camada**: Tests
- `tests/Domain.UnitTests/Entities/UsuarioTests.cs`:
  - `Criar_ComCargoSuperAdmin_TenantIdDeveSerNull`
  - `Criar_ComCargoAdmin_TenantIdDeveSerObrigatorio`
  - `RegistrarAcesso_DeveAtualizarUltimoAcesso`
  - `AlterarCargo_ParaSuperAdmin_DeveLancarDomainException`
  - `Desativar_SuperAdmin_DeveLancarDomainException`
  - `Desativar_UsuarioNormal_DeveDesativar`
- `tests/Application.UnitTests/Services/AuthServiceTests.cs`:
  - `LoginAsync_ComCredenciaisValidas_DeveRetornarTokens`
  - `LoginAsync_ComSenhaErrada_DeveLancarDomainException`
  - `LoginAsync_ComUsuarioInativo_DeveLancarDomainException`
  - `LoginAsync_ComTenantInativo_DeveLancarDomainException`
  - `RefreshTokenAsync_ComTokenValido_DeveRetornarNovosTokens`
  - `RefreshTokenAsync_ComTokenRevogado_DeveLancarDomainException`

---

## F2 — MULTI-TENANCY

### Contexto
Para SaaS, cada empresa (tenant) deve ver SOMENTE seus dados. Um tecnico da empresa A nao pode ver OS da empresa B.

### Decisao Arquitetural
- **Estrategia**: Banco compartilhado com coluna `TenantId` (GUID) em todas as entidades
- **Filtro global** do EF Core: `builder.HasQueryFilter(x => x.TenantId == currentTenantId)`
- **TenantId extraido do JWT** (claim `tenant_id`) em cada request
- **Nao criar banco separado por tenant** (custo proibitivo para SaaS de PME)
- **SuperAdmin ignora query filter** — ve dados de todos os tenants

### Tasks

- [ ] **F2-T1**: Entidade Tenant no Domain
**Camada**: Domain
**Arquivo**: `src/Domain/Entities/Tenant.cs`

```
Propriedades:
- Id: Guid
- Nome: string (required — nome da empresa)
- Slug: string (required, unique — identificador URL-safe, ex: "assistencia-tech")
- Documento: string? (CNPJ/CPF do dono)
- Email: string (required — email principal da empresa)
- Telefone: string?
- Endereco: string?
- Cidade: string?
- Estado: string? (UF — 2 chars)
- LogoUrl: string?
- PlanoAtual: PlanoTenant enum
- Ativo: bool
- MotivoDesativacao: string? (preenchido pelo SuperAdmin ao suspender)
- DataCriacao: DateTime
- DataExpiracao: DateTime? (trial/plano com vencimento)
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

**Enum `PlanoTenant`** em `Domain/Enums/`: `Free`, `Starter`, `Pro`, `Business`, `Enterprise`

**Interface `ITenantRepository`** em `Domain/Interfaces/`:
```csharp
Task AdicionarAsync(Tenant tenant, CancellationToken ct = default);
Task AtualizarAsync(Tenant tenant, CancellationToken ct = default);
Task<Tenant?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
Task<Tenant?> ObterPorSlugAsync(string slug, CancellationToken ct = default);
Task<bool> SlugExisteAsync(string slug, CancellationToken ct = default);
Task<IEnumerable<Tenant>> ListarTodosAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
Task<int> ContarAsync(CancellationToken ct = default);
```

**Regras**:
- Slug gerado a partir do Nome (lowercase, sem acentos, espacos viram hifens)
- Slug deve ser unico (validar no service antes de salvar)
- Metodo `Suspender(string motivo)` — seta `Ativo = false`, `MotivoDesativacao = motivo`
- Metodo `Reativar()` — seta `Ativo = true`, limpa motivo
- Metodo `AlterarPlano(PlanoTenant novoPlano)`
- Metodo `RenovarAte(DateTime novaExpiracao)`
- Metodo `AtualizarDados(nome, documento, email, telefone, endereco, cidade, estado)`

---

- [ ] **F2-T2**: Interface ITenantProvider no Domain
**Camada**: Domain
**Arquivo**: `src/Domain/Interfaces/ITenantProvider.cs`

```csharp
public interface ITenantProvider
{
    Guid? TenantId { get; }
    bool EhSuperAdmin { get; }
}
```

**IMPORTANTE**: `TenantId` e nullable. SuperAdmin tem `TenantId = null` e `EhSuperAdmin = true`. A implementacao na Infrastructure extrai isso do JWT.

---

- [ ] **F2-T3**: Adicionar TenantId em TODAS as entidades existentes
**Camada**: Domain
**Arquivos modificados**:
- `src/Domain/Entities/Cliente.cs` — adicionar `public Guid TenantId { get; private set; }`
- `src/Domain/Entities/Equipamento.cs` — adicionar `TenantId`
- `src/Domain/Entities/OrdemServico.cs` — adicionar `TenantId`
- (Filhos da OS como OrdemServicoServico NAO precisam de TenantId — sao acessados apenas via aggregate root que ja tem TenantId)

**Impacto nos factory methods `Criar()`**: Adicionar parametro `Guid tenantId` como primeiro argumento. Guard clause: `if (tenantId == Guid.Empty) throw new ArgumentException(...)`.

**IMPORTANTE**: Isso e uma breaking change. Todos os services na Application precisam injetar `ITenantProvider` e passar `tenantProvider.TenantId!.Value` ao criar entidades.

---

- [ ] **F2-T4**: Global Query Filter no EF Core
**Camada**: Infrastructure
**Arquivo modificado**: `src/Infrastructure/Persistence/AppDbContext.cs`

```csharp
public class AppDbContext : IdentityDbContext<AppIdentityUser>
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Filtro global: SOMENTE se nao for SuperAdmin
        // SuperAdmin precisa ver dados de todos os tenants
        if (!_tenantProvider.EhSuperAdmin && _tenantProvider.TenantId.HasValue)
        {
            var tenantId = _tenantProvider.TenantId.Value;
            modelBuilder.Entity<Cliente>().HasQueryFilter(x => x.TenantId == tenantId);
            modelBuilder.Entity<Equipamento>().HasQueryFilter(x => x.TenantId == tenantId);
            modelBuilder.Entity<OrdemServico>().HasQueryFilter(x => x.TenantId == tenantId);
            modelBuilder.Entity<Usuario>().HasQueryFilter(x => x.TenantId == tenantId);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Defense in depth: garantir TenantId nas insercoes
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added))
        {
            var tenantIdProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
            if (tenantIdProp is not null
                && tenantIdProp.CurrentValue is Guid id
                && id == Guid.Empty
                && _tenantProvider.TenantId.HasValue)
            {
                tenantIdProp.CurrentValue = _tenantProvider.TenantId.Value;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

---

- [ ] **F2-T5**: Implementacao do TenantProvider
**Camada**: Infrastructure
**Arquivo novo**: `src/Infrastructure/Tenancy/HttpTenantProvider.cs`

```csharp
public sealed class HttpTenantProvider : ITenantProvider
{
    public Guid? TenantId { get; }
    public bool EhSuperAdmin { get; }

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            TenantId = null;
            EhSuperAdmin = false;
            return;
        }

        var superAdminClaim = user.FindFirst("is_super_admin");
        EhSuperAdmin = superAdminClaim is not null && bool.Parse(superAdminClaim.Value);

        var tenantClaim = user.FindFirst("tenant_id");
        TenantId = tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tid)
            ? tid
            : null;
    }
}
```

Registrar no DI: `services.AddScoped<ITenantProvider, HttpTenantProvider>()`

---

- [ ] **F2-T6**: Migration
**Migration**: `AddMultiTenancy`

- Criar tabela `tenants` com todas as colunas
- Criar tabela `usuarios`
- Criar tabela `refresh_tokens`
- Adicionar coluna `TenantId` (CHAR(36), NOT NULL) nas tabelas: `clientes`, `equipamentos`, `ordens_servico`
- Adicionar FK de TenantId
- Criar indice composto `IX_{tabela}_TenantId` para performance
- Popular dados existentes com TenantId de um Tenant seed "default"

**Seed**: Criar Tenant "Sistema" com `PlanoTenant.Enterprise` para dados pre-existentes.

---

- [ ] **F2-T7**: Atualizar Services existentes
**Camada**: Application
**Arquivos modificados**:
- `src/Application/Services/ClienteService.cs` — injetar `ITenantProvider`, passar `TenantId` no `Criar()`
- `src/Application/Services/EquipamentoService.cs` — idem
- `src/Application/Services/OrdemServicoService.cs` — idem

**Padrao**:
```csharp
public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    // ...

    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantProvider.TenantId
            ?? throw new DomainException("Operacao requer um tenant valido.");

        var cliente = Cliente.Criar(tenantId, request.Nome, ...);
        // ...
    }
}
```

---

- [ ] **F2-T8**: Atualizar OrdemServicoAnotacao para rastrear UsuarioId
**Camada**: Domain
**Arquivo modificado**: `src/Domain/Entities/OrdemServicoAnotacao.cs`

O campo `Autor` hoje e uma `string` livre. Para SaaS com usuarios, deve referenciar o `UsuarioId`:

```
Antes:  public string Autor { get; private set; }
Depois: public Guid AutorId { get; private set; }
        public string AutorNome { get; private set; }  // desnormalizado para exibicao
```

Atualizar factory method `Criar()` e o `AdicionarAnotacaoRequest` para receber `Guid autorId` + `string autorNome`. O `autorId` vem do JWT claim `usuario_id`.

---

- [ ] **F2-T9**: Testes de Multi-Tenancy
- Testar que factory method com `tenantId == Guid.Empty` lanca excecao
- Testar que `ITenantProvider.EhSuperAdmin` desabilita query filter
- Testar que `SaveChangesAsync` preenche TenantId quando vazio (defense in depth)
- Testar isolamento: dados do Tenant A nao aparecem em query do Tenant B

---

## F3 — USUARIOS, ROLES E SUPER ADMIN

### Contexto
Dois niveis de gestao: SuperAdmin gerencia tenants, Admin de cada tenant gerencia seus usuarios.

### Tasks

- [ ] **F3-T1**: Tabela de Permissoes por Cargo

| Cargo | Criar OS | Aprovar OS | Ver financeiro | Gerenciar usuarios | Alterar plano | Gerenciar tenants | Impersonar |
|-------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **SuperAdmin** | — | — | Global | — | Sim | Sim | Sim |
| **Admin** | Sim | Sim | Sim | Sim (do tenant) | Sim (do tenant) | Nao | Nao |
| **Gerente** | Sim | Sim | Sim | Nao | Nao | Nao | Nao |
| **Tecnico** | Sim (proprias) | Nao | Nao | Nao | Nao | Nao | Nao |
| **Atendente** | Sim | Nao | Parcial (sem lucro) | Nao | Nao | Nao | Nao |

---

- [ ] **F3-T2**: Service de Gestao de Usuarios (Tenant-scoped)
**Camada**: Application
**Arquivos novos**:
- `src/Application/Interfaces/IUsuarioService.cs`
- `src/Application/Services/UsuarioService.cs`
- `src/Application/DTOs/Usuarios/UsuarioResponse.cs`
- `src/Application/DTOs/Usuarios/CriarUsuarioRequest.cs`
- `src/Application/DTOs/Usuarios/AtualizarUsuarioRequest.cs`
- `src/Application/DTOs/Usuarios/Mappings/UsuarioMappings.cs`
- `src/Application/DTOs/Usuarios/Validators/CriarUsuarioValidator.cs`

**Operacoes**:
```
CriarAsync(CriarUsuarioRequest) → UsuarioResponse   // Cria Identity + Usuario domain
AtualizarAsync(Guid, AtualizarUsuarioRequest) → void
ListarPorTenantAsync(PagedRequest) → PagedResponse<UsuarioResponse>
ObterPorIdAsync(Guid) → UsuarioResponse?
DesativarAsync(Guid) → void
ReativarAsync(Guid) → void
AlterarCargoAsync(Guid, CargoUsuario) → void
```

**Validacoes**:
- Nao pode criar usuario com cargo `SuperAdmin` (somente seed)
- Nao pode criar mais usuarios que o plano permite (usar PlanoLimiteService)
- Email deve ser unico no tenant
- Admin do tenant A nao pode ver/editar usuarios do tenant B (query filter garante)

---

- [ ] **F3-T3**: Painel do Super Admin
**Camada**: Application
**Arquivos novos**:
- `src/Application/Interfaces/ISuperAdminService.cs`
- `src/Application/Services/SuperAdminService.cs`
- `src/Application/DTOs/Admin/TenantResumoResponse.cs`
- `src/Application/DTOs/Admin/TenantDetalheResponse.cs`
- `src/Application/DTOs/Admin/MetricasGlobaisResponse.cs`
- `src/Application/DTOs/Admin/ImpersonarRequest.cs`

**ISuperAdminService**:
```csharp
// Gestao de Tenants
Task<PagedResponse<TenantResumoResponse>> ListarTenantsAsync(PagedRequest request, CancellationToken ct = default);
Task<TenantDetalheResponse?> ObterTenantAsync(Guid tenantId, CancellationToken ct = default);
Task SuspenderTenantAsync(Guid tenantId, string motivo, CancellationToken ct = default);
Task ReativarTenantAsync(Guid tenantId, CancellationToken ct = default);
Task AlterarPlanoTenantAsync(Guid tenantId, PlanoTenant novoPlano, CancellationToken ct = default);

// Metricas Globais
Task<MetricasGlobaisResponse> ObterMetricasAsync(CancellationToken ct = default);

// Impersonacao (login como outro usuario para suporte)
Task<LoginResponse> ImpersonarAsync(ImpersonarRequest request, CancellationToken ct = default);
```

**MetricasGlobaisResponse**:
```csharp
public sealed record MetricasGlobaisResponse(
    int TotalTenants,
    int TotalTenantsAtivos,
    int TotalUsuarios,
    int TotalOsGlobal,
    decimal MrrEstimado,                        // Monthly Recurring Revenue
    IReadOnlyList<TenantsPorPlanoResponse> TenantsPorPlano,
    IReadOnlyList<TenantResumoResponse> UltimosTenantsRegistrados
);
```

**Impersonacao**:
- SuperAdmin pode gerar um JWT com o TenantId de qualquer tenant
- O JWT gerado inclui claim `impersonated_by: superAdminUsuarioId`
- Permite resolver problemas de clientes sem pedir credenciais
- Registrar no audit trail que houve impersonacao

---

- [ ] **F3-T4**: Endpoints do Super Admin
**Camada**: Api
**Arquivo novo**: `src/Api/Endpoints/SuperAdminEndpoints.cs`

```
GET    /api/admin/tenants                   → ListarTenantsAsync
GET    /api/admin/tenants/{id:guid}         → ObterTenantAsync
POST   /api/admin/tenants/{id:guid}/suspender  → SuspenderTenantAsync
POST   /api/admin/tenants/{id:guid}/reativar   → ReativarTenantAsync
PATCH  /api/admin/tenants/{id:guid}/plano      → AlterarPlanoTenantAsync
GET    /api/admin/metricas                  → ObterMetricasAsync
POST   /api/admin/impersonar               → ImpersonarAsync
```

Todos com `.RequireAuthorization("SuperAdminOnly")`.

---

- [ ] **F3-T5**: Endpoints de Usuarios (Tenant-scoped)
**Camada**: Api
**Arquivo novo**: `src/Api/Endpoints/UsuarioEndpoints.cs`

```
GET    /api/usuarios                    → ListarPorTenantAsync   [Admin]
GET    /api/usuarios/{id:guid}          → ObterPorIdAsync        [Admin, Gerente]
POST   /api/usuarios                    → CriarAsync             [Admin]
PUT    /api/usuarios/{id:guid}          → AtualizarAsync         [Admin]
PATCH  /api/usuarios/{id:guid}/cargo    → AlterarCargoAsync      [Admin]
DELETE /api/usuarios/{id:guid}          → DesativarAsync         [Admin]
POST   /api/usuarios/{id:guid}/reativar → ReativarAsync          [Admin]
```

---

- [ ] **F3-T6**: Authorization Policies
**Camada**: Api
**Arquivo modificado**: `src/Api/Extensions/ServiceCollectionExtensions.cs`

```csharp
services.AddAuthorizationBuilder()
    .AddPolicy("SuperAdminOnly", p => p.RequireRole("SuperAdmin"))
    .AddPolicy("AdminOnly", p => p.RequireRole("Admin", "SuperAdmin"))
    .AddPolicy("GerenteOuAdmin", p => p.RequireRole("Admin", "Gerente", "SuperAdmin"))
    .AddPolicy("PodeAlterarStatus", p => p.RequireRole("Admin", "Gerente", "Tecnico"))
    .AddPolicy("PodeCriarOS", p => p.RequireRole("Admin", "Gerente", "Tecnico", "Atendente"));
```

**Aplicar nos endpoints existentes**:
- `MapOrdemServicoEndpoints`: `.RequireAuthorization("PodeCriarOS")` nos POSTs, `.RequireAuthorization("PodeAlterarStatus")` no PATCH de status
- `MapClienteEndpoints`: `.RequireAuthorization()` (qualquer autenticado)
- `MapEquipamentoEndpoints`: `.RequireAuthorization()` (qualquer autenticado)

---

- [ ] **F3-T7**: Tela do Super Admin no Blazor
**Camada**: Web
**Arquivos novos**:
- `src/Web/Pages/Admin/AdminDashboardPage.razor` (rota: `/admin`)
- `src/Web/Pages/Admin/AdminTenantsPage.razor` (rota: `/admin/tenants`)
- `src/Web/Pages/Admin/AdminTenantDetalhePage.razor` (rota: `/admin/tenants/{id}`)
- `src/Web/ViewModels/Admin/AdminDashboardViewModel.cs`
- `src/Web/ViewModels/Admin/AdminTenantsViewModel.cs`
- `src/Web/Services/Api/ISuperAdminApi.cs`
- `src/Web/Services/Api/SuperAdminApi.cs`

**Funcionalidades do painel admin**:
- Dashboard com MRR, total tenants, total usuarios, total OS global
- Lista de tenants (nome, plano, status, total usuarios, total OS, data cadastro)
- Detalhe do tenant (dados, usuarios, metricas, botao suspender/reativar, alterar plano)
- Botao "Impersonar" para acessar como Admin do tenant

**Visibilidade**: Somente para usuarios com cargo `SuperAdmin`. Menu separado.

---

- [ ] **F3-T8**: Tela de Usuarios do Tenant no Blazor
**Camada**: Web
**Arquivos novos**:
- `src/Web/Pages/Usuarios/UsuariosPage.razor`
- `src/Web/ViewModels/Usuarios/UsuariosViewModel.cs`
- `src/Web/ViewModels/Usuarios/UsuarioFormModel.cs`
- `src/Web/Services/Api/IUsuariosApi.cs`
- `src/Web/Services/Api/UsuariosApi.cs`

**Funcionalidades**:
- Listar usuarios do tenant (DataGrid)
- Criar novo usuario (nome, email, senha, cargo)
- Editar cargo
- Ativar/Desativar usuario
- Mostrar ultimo acesso
- Indicador visual de limite do plano (ex: "3/3 usuarios utilizados")

**Visibilidade**: Somente para `Admin`.

---

## F4 — AUDIT TRAIL (LOG DE ATIVIDADES)

### Contexto
Para SaaS profissional e compliance, e necessario saber **quem fez o que e quando**. Hoje `OrdemServicoAnotacao.Autor` e uma string livre — nao ha rastreabilidade real.

### Decisao Arquitetural
- Tabela `audit_logs` separada, NUNCA deletada (append-only)
- Registrar automaticamente via interceptor do EF Core (SaveChanges)
- Nao faz parte do dominio — e infra pura (cross-cutting concern)

### Tasks

- [ ] **F4-T1**: Entidade AuditLog
**Camada**: Infrastructure (NAO Domain — audit e infra)
**Arquivo novo**: `src/Infrastructure/Audit/AuditLog.cs`

```csharp
public sealed class AuditLog
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? UsuarioId { get; init; }
    public string UsuarioNome { get; init; } = string.Empty;
    public string Entidade { get; init; } = string.Empty;       // "OrdemServico", "Cliente"
    public string EntidadeId { get; init; } = string.Empty;     // GUID da entidade
    public string Acao { get; init; } = string.Empty;           // "Criou", "Atualizou", "AlterouStatus"
    public string? Detalhes { get; init; }                      // JSON com old/new values
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

---

- [ ] **F4-T2**: SaveChanges Interceptor
**Camada**: Infrastructure
**Arquivo novo**: `src/Infrastructure/Audit/AuditSaveChangesInterceptor.cs`

Usar `SaveChangesInterceptor` do EF Core para capturar todas as mudancas:
- Entidades adicionadas: registrar `Acao = "Criou"`
- Entidades modificadas: registrar `Acao = "Atualizou"` + JSON das propriedades alteradas
- Extrair `UsuarioId` e `TenantId` do `ITenantProvider` e `IHttpContextAccessor`

Registrar o interceptor no DI:
```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseMySql(...)
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
```

---

- [ ] **F4-T3**: Endpoint e Tela de Audit
**Camada**: Api + Web

```
GET /api/audit-logs?entidade=OrdemServico&entidadeId={id}&page=1  → PagedResponse<AuditLogResponse>
```

Acesso: `Admin` (ve do seu tenant), `SuperAdmin` (ve de todos).

Na `OrdemServicoDetalhePage.razor`: adicionar aba "Historico" mostrando quem alterou a OS e quando.

---

## F5 — GERACAO DE PDF

(Sem mudancas em relacao a versao anterior)

### Tasks

- [ ] **F5-T1**: Interface de Geracao de PDF no Application
**Arquivo**: `src/Application/Interfaces/IPdfService.cs`

```csharp
public interface IPdfService
{
    Task<byte[]> GerarOrcamentoAsync(Guid ordemServicoId, CancellationToken ct = default);
    Task<byte[]> GerarOrdemServicoAsync(Guid ordemServicoId, CancellationToken ct = default);
    Task<byte[]> GerarReciboAsync(Guid ordemServicoId, CancellationToken ct = default);
}
```

- [ ] **F5-T2**: Implementacao com QuestPDF
**Pacote NuGet**: `QuestPDF` no Infrastructure.csproj

Templates (cada um como classe separada em `Infrastructure/Pdf/Templates/`):
- Header: Logo da empresa (Tenant.LogoUrl) + dados + numero OS + data
- Dados do cliente + equipamento
- Tabelas: servicos, produtos, taxas
- Subtotal, desconto, total
- Condicoes de pagamento / pagamentos registrados
- Rodape com validade e espaco para assinatura

- [ ] **F5-T3**: Endpoints de PDF
```
GET /api/ordens-servico/{id:guid}/pdf/orcamento
GET /api/ordens-servico/{id:guid}/pdf/os
GET /api/ordens-servico/{id:guid}/pdf/recibo
```

Verificar `PlanoLimiteService.TemPdf` antes de gerar (plano Free nao tem).

- [ ] **F5-T4**: Botoes de PDF no Blazor
Adicionar na `OrdemServicoDetalhePage.razor`. Esconder botoes se plano nao suporta.

---

## F6 — NOTIFICACOES POR EMAIL

### Tasks

- [ ] **F6-T1**: Interface e DTOs
```csharp
public interface IEmailService
{
    Task EnviarAsync(EmailMessage message, CancellationToken ct = default);
    Task EnviarTemplateAsync(string template, Dictionary<string, string> variaveis, string para, string assunto, CancellationToken ct = default);
}
```

- [ ] **F6-T2**: Implementacao SMTP/SendGrid
**Arquivo**: `src/Infrastructure/Email/SmtpEmailService.cs`

**EmailOptions** (appsettings): Host, Port, User, Password, FromAddress, FromName, HabilitarEnvio (bool — false em dev)

**Templates HTML** (metodos retornando string interpolada):
- `OrcamentoEnviado` — "Ola {cliente}, seu orcamento #{numero} no valor de {total} esta disponivel."
- `StatusAlterado` — "Sua OS #{numero} mudou de {anterior} para {novo}."
- `PagamentoRegistrado` — "Recebemos pagamento de {valor}. Restam {restante}."
- `BoasVindas` — "Bem-vindo ao OrdemServico! Sua conta foi criada."
- `RedefinicaoSenha` — "Clique no link para redefinir sua senha: {link}"

- [ ] **F6-T3**: Disparar nos Services existentes
**Regra**: Email e best-effort. Wrapar em try-catch, logar falhas, NUNCA bloquear a operacao.

Verificar `PlanoLimiteService.TemEmail` antes de enviar (plano Free nao tem).

---

## F7 — DASHBOARD COM KPIs

### Tasks

- [ ] **F7-T1**: Service + Repository de Dashboard
**DTOs**:
```csharp
public sealed record DashboardResponse(
    int TotalOsAbertas,
    int TotalOsEmAndamento,
    int TotalOsConcluidas,
    int TotalOsEntregues,
    int TotalOsMes,
    decimal FaturamentoMes,
    decimal FaturamentoMesAnterior,
    decimal TicketMedio,
    decimal TaxaConclusao,
    IReadOnlyList<OsPorStatusResponse> OsPorStatus,
    IReadOnlyList<FaturamentoDiarioResponse> FaturamentoUltimos30Dias
);
```

Queries otimizadas com `.AsNoTracking()`, `GroupBy`, `Sum`, projecoes (nao carregar entidades inteiras).

- [ ] **F7-T2**: Endpoint
```
GET /api/dashboard → DashboardResponse [Admin, Gerente]
```

- [ ] **F7-T3**: Pagina Dashboard no Blazor
Transformar `Home.razor` no dashboard com Radzen: cards KPI, donut por status, barra faturamento 30 dias.

Mostrar dashboard diferente para SuperAdmin (metricas globais via `/api/admin/metricas`).

---

## F8 — ONBOARDING + BILLING

### Tasks

- [ ] **F8-T1**: Signup Self-Service
**Endpoint publico**: `POST /api/onboarding/signup`

**Fluxo**:
1. Validar email unico (nao existe em nenhum tenant)
2. Gerar slug a partir do nome da empresa
3. Criar `Tenant`
4. Criar `AppIdentityUser`
5. Criar `Usuario` com cargo `Admin`
6. Atribuir role "Admin" no Identity
7. Gerar JWT + RefreshToken
8. Enviar email de boas-vindas (se TemEmail)
9. Retornar tokens + dados do tenant

- [ ] **F8-T2**: Pagina de Signup
Formulario: Nome da empresa, Email, Senha, Confirmar senha, Nome do responsavel. Selecao de plano com cards visuais mostrando features de cada plano.

- [ ] **F8-T3**: Billing Service
```
GET  /api/billing/plano     → PlanoResponse [Admin]
PUT  /api/billing/plano     → AlterarPlanoAsync [Admin]
GET  /api/billing/planos    → ListarPlanosDisponiveisAsync [publico]
```

Nesta fase, cobranca manual ou link externo. Integracao com gateway fica para Fase 2.

- [ ] **F8-T4**: Limites por Plano (Enforcement)
```csharp
public static PlanoLimites ObterLimites(PlanoTenant plano) => plano switch
{
    PlanoTenant.Free     => new(MaxUsuarios: 1,  MaxOsMes: 20,   TemPdf: false, TemEmail: false),
    PlanoTenant.Starter  => new(MaxUsuarios: 3,  MaxOsMes: 100,  TemPdf: true,  TemEmail: true),
    PlanoTenant.Pro      => new(MaxUsuarios: 10, MaxOsMes: null, TemPdf: true,  TemEmail: true),
    PlanoTenant.Business => new(MaxUsuarios: null, MaxOsMes: null, TemPdf: true, TemEmail: true),
    _ => throw new DomainException("Plano invalido.")
};
```

Enforcement nos services: checar antes de `CriarAsync()` em OS e Usuario. Lancar `DomainException("Limite do plano atingido. Faca upgrade.")`.

- [ ] **F8-T5**: Pagina Configuracoes / Minha Conta
Secoes: Dados da Empresa, Plano Atual (com limites e uso), Minha Conta (alterar senha), Link para Usuarios.

---

## CHECKLIST GERAL DE BUILD

Apos implementar cada feature:

- [ ] `dotnet build OrdemServico.sln` — 0 warnings, 0 erros
- [ ] `dotnet test` unitarios — todos passando
- [ ] Todas as classes de implementacao sao `sealed`
- [ ] Todos os DTOs sao `record`
- [ ] Todos os metodos async tem `CancellationToken`
- [ ] Nomes de negocio em Portugues
- [ ] Repositories read-only usam `.AsNoTracking()`
- [ ] Migration gerada e aplicada
- [ ] Endpoints tem `ValidationFilter`, `.Produces()`, `.ProducesProblem()`
- [ ] ViewModels herdam `ViewModelBase`, usam `SetProperty()`
- [ ] Endpoints protegidos com `.RequireAuthorization()`
- [ ] SuperAdmin nao tem TenantId
- [ ] Query filter desabilitado para SuperAdmin
- [ ] Docker build passa

---

## GRAFO DE DEPENDENCIAS

```
F0-T1 (Serilog pacotes) → F0-T2 (Program.cs) → F0-T3 (appsettings)
  ↓
F0-T4 (Remover RequestLoggingMiddleware) → F0-T5 (Enricher) → F0-T6 (Logs nos services)
  ↓
F0-T7 (Serilog Web) → F0-T8 (Seq docker)
  ↓
F1-T1 (Usuario entity) → F1-T2 (enum Cargo)
  ↓
F1-T3 (Identity) → F1-T4 (AuthService) → F1-T5 (JWT) → F1-T6 (Endpoints)
  ↓                                                        ↓
F1-T7 (Blazor auth)                                     F1-T8 (Seed SuperAdmin)
  ↓                                                        ↓
F1-T9 (Testes)
  ↓
F2-T1 (Tenant entity) → F2-T2 (ITenantProvider) → F2-T3 (TenantId entidades)
  ↓
F2-T4 (Query filter + SaveChanges) → F2-T5 (HttpTenantProvider) → F2-T6 (Migration)
  ↓
F2-T7 (Services) → F2-T8 (Anotacao com UsuarioId) → F2-T9 (Testes)
  ↓
F3-T1 (Permissoes) → F3-T2 (UsuarioService) → F3-T3 (SuperAdminService)
  ↓
F3-T4 (Endpoints SuperAdmin) → F3-T5 (Endpoints Usuarios) → F3-T6 (Policies)
  ↓
F3-T7 (Tela SuperAdmin) → F3-T8 (Tela Usuarios)
  ↓
F4 (Audit Trail)
  ↓
F5 (PDF), F6 (Email), F7 (Dashboard) → em paralelo
  ↓
F8 (Onboarding + Billing)
```

---

## PACOTES NUGET

| Projeto | Pacote | Motivo |
|---------|--------|--------|
| Api | `Serilog.AspNetCore` | Logging estruturado + request logging |
| Api | `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT auth middleware |
| Infrastructure | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity tables + UserManager |
| Infrastructure | `Serilog.Sinks.Seq` | Sink para Seq (log viewer) |
| Infrastructure | `Serilog.Sinks.File` | Sink para arquivo com rolling |
| Infrastructure | `QuestPDF` | Geracao de PDF |
| Web | `Serilog.AspNetCore` | Logging estruturado no Blazor |
| Domain | Nenhum novo | Continua com zero deps |
| Application | Nenhum novo | Continua apenas FluentValidation |

---

## IMPACTO NAS ENTIDADES EXISTENTES

| Entidade | Mudanca |
|----------|---------|
| Cliente | + `TenantId` (Guid, required). `Criar()` ganha parametro `tenantId`. |
| Equipamento | + `TenantId`. `Criar()` ganha `tenantId`. |
| OrdemServico | + `TenantId`. `Criar()` ganha `tenantId`. |
| OrdemServicoAnotacao | `Autor` (string) → `AutorId` (Guid) + `AutorNome` (string). |
| OrdemServicoServico | Sem mudanca. |
| OrdemServicoProduto | Sem mudanca. |
| OrdemServicoTaxa | Sem mudanca. |
| OrdemServicoPagamento | Sem mudanca. |
| OrdemServicoFoto | Sem mudanca. |

## IMPACTO NOS SERVICES

| Service | Mudanca |
|---------|---------|
| ClienteService | + `ITenantProvider`. Passar `TenantId` no `Criar()`. |
| EquipamentoService | + `ITenantProvider`. Passar `TenantId` no `Criar()`. |
| OrdemServicoService | + `ITenantProvider`. Passar `TenantId` no `Criar()`. `AdicionarAnotacaoAsync` recebe `UsuarioId`. |

## IMPACTO NOS TESTES

| Projeto | Mudanca |
|---------|---------|
| Domain.UnitTests | Factory methods agora exigem `tenantId`. Novos testes para Usuario, Tenant. |
| Application.UnitTests | Mock de `ITenantProvider`. Novos testes para Auth, UsuarioService, SuperAdminService. |
| Web.UnitTests | Sem impacto direto (Web nao referencia Domain). |

---

## CONFIGURACAO (appsettings.json)

Novas secoes necessarias:

```json
{
  "Jwt": {
    "SecretKey": "CHAVE_SECRETA_MIN_32_CHARS_TROCAR_EM_PRODUCAO",
    "Issuer": "OrdemServico",
    "Audience": "OrdemServico",
    "ExpiracaoMinutos": 60,
    "RefreshExpiracaoDias": 7
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "",
    "Password": "",
    "FromAddress": "noreply@ordemservico.com",
    "FromName": "OrdemServico",
    "HabilitarEnvio": false
  },
  "SuperAdmin": {
    "Email": "admin@ordemservico.com",
    "Password": "Admin@123456"
  }
}
```

**Em producao**: `Jwt:SecretKey`, `Email:Password` e `SuperAdmin:Password` devem vir de **variaveis de ambiente** ou **Azure Key Vault / AWS Secrets Manager**.

---

## RESUMO DE ARQUIVOS

| Camada | Arquivos Novos | Modificados | Removidos |
|--------|:-:|:-:|:-:|
| Domain | ~10 | ~5 | 0 |
| Application | ~30 | ~6 | 0 |
| Infrastructure | ~19 | ~6 | 0 |
| Api | ~6 | ~5 | 1 (RequestLoggingMiddleware) |
| Web | ~25 | ~7 | 0 |
| Tests | ~12 | ~8 | 0 |
| **Total** | **~102** | **~37** | **1** |
