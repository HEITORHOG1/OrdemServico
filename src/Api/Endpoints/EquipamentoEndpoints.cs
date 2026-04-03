using Api.Filters;
using Application.DTOs.Clientes;
using Application.DTOs.Equipamentos;
using Application.Interfaces;

namespace Api.Endpoints;

public static class EquipamentoEndpoints
{
    public static void MapEquipamentoEndpoints(this IEndpointRouteBuilder routes)
    {
        var grid = routes.MapGroup("/api/equipamentos").WithTags("Equipamentos").RequireAuthorization();

        grid.MapPost("/", async (CriarEquipamentoRequest request, IEquipamentoService service, CancellationToken ct) =>
        {
            var response = await service.CriarAsync(request, ct);
            return Results.Created($"/api/equipamentos/{response.Id}", response);
        })
        .AddEndpointFilter<ValidationFilter<CriarEquipamentoRequest>>()
        .WithName("CriarEquipamento")
        .Produces<EquipamentoResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        grid.MapGet("/cliente/{clienteId:guid}", async (Guid clienteId, IEquipamentoService service, CancellationToken ct) =>
        {
            var response = await service.ListarPorClienteIdAsync(clienteId, ct);
            return Results.Ok(response);
        })
        .WithName("ListarEquipamentosPorCliente")
        .Produces<IEnumerable<EquipamentoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
