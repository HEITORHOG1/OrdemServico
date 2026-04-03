using Api.Filters;
using Application.DTOs.Auth;
using Application.Interfaces;

namespace Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Autenticacao");

        group.MapPost("/login", async (LoginRequest request, IAuthService service, CancellationToken ct) =>
        {
            var response = await service.LoginAsync(request, ct);
            return Results.Ok(response);
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>()
        .WithName("Login")
        .AllowAnonymous()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService service, CancellationToken ct) =>
        {
            var response = await service.RefreshTokenAsync(request, ct);
            return Results.Ok(response);
        })
        .WithName("RefreshToken")
        .AllowAnonymous()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/registrar", async (RegistrarUsuarioRequest request, IAuthService service, HttpContext http, CancellationToken ct) =>
        {
            var tenantIdClaim = http.User.FindFirst("tenant_id")?.Value;
            Guid? tenantId = !string.IsNullOrWhiteSpace(tenantIdClaim) ? Guid.Parse(tenantIdClaim) : null;

            var response = await service.RegistrarAsync(request, tenantId, ct);
            return Results.Created($"/api/auth/me", response);
        })
        .AddEndpointFilter<ValidationFilter<RegistrarUsuarioRequest>>()
        .WithName("RegistrarUsuario")
        .RequireAuthorization("AdminOuSuperAdmin")
        .Produces<UsuarioResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/alterar-senha", async (AlterarSenhaRequest request, IAuthService service, HttpContext http, CancellationToken ct) =>
        {
            var usuarioId = Guid.Parse(http.User.FindFirst("usuario_id")!.Value);
            await service.AlterarSenhaAsync(usuarioId, request, ct);
            return Results.NoContent();
        })
        .AddEndpointFilter<ValidationFilter<AlterarSenhaRequest>>()
        .WithName("AlterarSenha")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/esqueci-senha", async (EsqueciSenhaRequest request, IAuthService service, CancellationToken ct) =>
        {
            await service.EsqueciSenhaAsync(request, ct);
            return Results.Ok(new { message = "Se o email estiver cadastrado, um link de redefinicao sera enviado." });
        })
        .WithName("EsqueciSenha")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);

        group.MapPost("/redefinir-senha", async (RedefinirSenhaRequest request, IAuthService service, CancellationToken ct) =>
        {
            await service.RedefinirSenhaAsync(request, ct);
            return Results.Ok(new { message = "Senha redefinida com sucesso." });
        })
        .AddEndpointFilter<ValidationFilter<RedefinirSenhaRequest>>()
        .WithName("RedefinirSenha")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/me", async (IAuthService service, HttpContext http, CancellationToken ct) =>
        {
            var usuarioId = Guid.Parse(http.User.FindFirst("usuario_id")!.Value);
            var response = await service.ObterUsuarioAtualAsync(usuarioId, ct);
            return response is not null ? Results.Ok(response) : Results.NotFound();
        })
        .WithName("ObterUsuarioAtual")
        .RequireAuthorization()
        .Produces<UsuarioResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
