---
name: criar-endpoint
description: Cria um novo endpoint Minimal API seguindo os padroes do projeto - com MapGroup, ValidationFilter, ProblemDetails e metadata OpenAPI.
disable-model-invocation: true
user-invocable: true
argument-hint: "[nome-do-recurso]"
---

# Criar Endpoint Minimal API — $ARGUMENTS

Crie os endpoints para **$ARGUMENTS** em `src/Api/Endpoints/` seguindo o padrao Minimal API do projeto.

## Estrutura Obrigatoria

```csharp
using Api.Filters;
using Application.DTOs.Comuns;
using Application.DTOs.{Feature};
using Application.Interfaces;

namespace Api.Endpoints;

public static class {Feature}Endpoints
{
    public static void Map{Feature}Endpoints(this IEndpointRouteBuilder routes)
    {
        var grid = routes.MapGroup("/api/{recurso-plural-kebab-case}").WithTags("{Nome Legivel}");

        // POST / — Criar
        grid.MapPost("/", async (CriarXxxRequest request, IXxxService service, CancellationToken ct) =>
        {
            var response = await service.CriarAsync(request, ct);
            return Results.Created($"/api/{recurso}/{response.Id}", response);
        })
        .AddEndpointFilter<ValidationFilter<CriarXxxRequest>>()
        .WithName("CriarXxx")
        .Produces<XxxResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /{id:guid} — Obter por ID
        grid.MapGet("/{id:guid}", async (Guid id, IXxxService service, CancellationToken ct) =>
        {
            var response = await service.ObterPorIdAsync(id, ct);
            return response is not null ? Results.Ok(response) : Results.NotFound();
        })
        .WithName("ObterXxx")
        .Produces<XxxResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET / — Listar paginado
        grid.MapGet("/", async ([AsParameters] PagedRequest request, IXxxService service, CancellationToken ct) =>
        {
            var response = await service.ListarPaginadoAsync(request, ct);
            return Results.Ok(response);
        })
        .WithName("ListarXxx")
        .Produces<PagedResponse<XxxResumoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // PUT /{id:guid} — Atualizar
        grid.MapPut("/{id:guid}", async (Guid id, AtualizarXxxRequest request, IXxxService service, CancellationToken ct) =>
        {
            await service.AtualizarAsync(id, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AtualizarXxxRequest>>()
        .WithName("AtualizarXxx")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
```

## Regras

- **Rotas em kebab-case**: `/api/ordens-servico`, `/api/clientes`
- **Sub-recursos via POST**: `/api/ordens-servico/{id:guid}/servicos`
- **Status change via PATCH**: `/api/ordens-servico/{id:guid}/status`
- **ValidationFilter** em todo endpoint que recebe body (POST/PUT)
- **Produces** declarados para TODOS os status codes possiveis
- **CancellationToken** em todos os handlers
- **Registrar** o novo endpoint em `WebApplicationExtensions.cs`

## Respostas HTTP Padrao

| Operacao | Verbo | Retorno |
|----------|-------|---------|
| Criar | POST | 201 Created com body + Location header |
| Obter | GET | 200 OK com body / 404 Not Found |
| Listar | GET | 200 OK com PagedResponse |
| Atualizar | PUT | 204 No Content |
| Mutacao parcial | POST/PATCH | 204 No Content |
| Erro de validacao | - | 400 Bad Request (ValidationProblem) |
| Erro de dominio | - | 422 Unprocessable Entity |
| Concorrencia | - | 409 Conflict |
