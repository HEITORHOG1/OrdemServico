using Api.Filters;
using Application.DTOs.Comuns;
using Application.DTOs.OrdemServicos;
using Application.Interfaces;

namespace Api.Endpoints;

public static class OrdemServicoEndpoints
{
    public static void MapOrdemServicoEndpoints(this IEndpointRouteBuilder routes)
    {
        var grid = routes.MapGroup("/api/ordens-servico").WithTags("Ordens de Serviço").RequireAuthorization();

        grid.MapPost("/", async (CriarOrdemServicoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            var response = await service.CriarAsync(request, ct);
            return Results.Created($"/api/ordens-servico/{response.Id}", response);
        })
        .AddEndpointFilter<ValidationFilter<CriarOrdemServicoRequest>>()
        .WithName("CriarOS")
        .Produces<OrdemServicoResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        grid.MapGet("/{id:guid}", async (Guid id,  IOrdemServicoService service, CancellationToken ct) =>
        {
            var response = await service.ObterPorIdAsync(id, ct);
            return response is not null ? Results.Ok(response) : Results.NotFound();
        })
        .WithName("ObterOS")
        .Produces<OrdemServicoResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapGet("/", async ([AsParameters] PagedRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            var response = await service.ListarPaginadoAsync(request, ct);
            return Results.Ok(response);
        })
        .WithName("ListarOS")
        .Produces<PagedResponse<OrdemServicoResumoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPut("/{id:guid}", async (Guid id, AtualizarOrdemServicoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AtualizarAsync(id, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AtualizarOrdemServicoRequest>>()
        .WithName("AtualizarOSBase")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPost("/{id:guid}/servicos", async (Guid id, AdicionarServicoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AdicionarServicoAsync(id, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AdicionarServicoRequest>>()
        .WithName("AddServico")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPost("/{id:guid}/produtos", async (Guid id, AdicionarProdutoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AdicionarProdutoAsync(id, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AdicionarProdutoRequest>>()
        .WithName("AddProduto")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPost("/{id:guid}/desconto", async (Guid id, AplicarDescontoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AplicarDescontoAsync(id, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AplicarDescontoRequest>>()
        .WithName("AplicarDesconto")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPost("/{id:guid}/taxas", async (Guid id, AdicionarTaxaRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AdicionarTaxaAsync(id, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AdicionarTaxaRequest>>()
        .WithName("AddTaxa")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPost("/{id:guid}/pagamentos", async (Guid id, RegistrarPagamentoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.RegistrarPagamentoAsync(id, request, ct);
            return Results.NoContent();
        })
        .WithName("AddPagamento")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPost("/{id:guid}/anotacoes", async (Guid id, AdicionarAnotacaoRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AdicionarAnotacaoAsync(id, request, ct);
            return Results.NoContent();
        })
        .WithName("AddAnotacao")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapPatch("/{id:guid}/status", async (Guid id, AlterarStatusRequest request, IOrdemServicoService service, CancellationToken ct) =>
        {
            await service.AlterarStatusAsync(id, request, ct);
            return Results.NoContent();
        })
        .WithName("AlterarStatus")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
