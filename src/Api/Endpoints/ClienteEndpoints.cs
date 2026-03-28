using Api.Filters;
using Application.DTOs.Clientes;
using Application.Interfaces;

namespace Api.Endpoints;

public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this IEndpointRouteBuilder routes)
    {
        var grid = routes.MapGroup("/api/clientes").WithTags("Clientes");

        grid.MapPost("/", async (CriarClienteRequest request, IClienteService service, CancellationToken ct) =>
        {
            var response = await service.CriarAsync(request, ct);
            return Results.Created($"/api/clientes/{response.Id}", response);
        })
        .AddEndpointFilter<ValidationFilter<CriarClienteRequest>>()
        .WithName("CriarCliente")
        .Produces<ClienteResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapGet("/{id:guid}", async (Guid id, IClienteService service, CancellationToken ct) =>
        {
            var response = await service.ObterPorIdAsync(id, ct);
            return response is not null ? Results.Ok(response) : Results.NotFound();
        })
        .WithName("ObterClientePorId")
        .Produces<ClienteResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapGet("/busca", async (string nome, IClienteService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(nome))
                return Results.Ok(Enumerable.Empty<ClienteResponse>());

            var response = await service.BuscarPorNomeAsync(nome, ct);
            return Results.Ok(response);
        })
        .WithName("BuscarClientesPorNome")
        .Produces<IEnumerable<ClienteResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
