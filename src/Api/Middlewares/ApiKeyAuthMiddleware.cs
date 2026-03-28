using Api.Options;
using Microsoft.Extensions.Options;

namespace Api.Middlewares;

public sealed class ApiKeyAuthMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly AuthOptions _authOptions;

    public ApiKeyAuthMiddleware(RequestDelegate next, IOptions<AuthOptions> authOptions)
    {
        _next = next;
        _authOptions = authOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_authOptions.IsApiKeyRequired)
        {
            await _next(context);
            return;
        }

        if (IsPublicPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey)
            || !string.Equals(providedKey.ToString(), _authOptions.ApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Nao autorizado",
                detail = "Chave de API invalida ou ausente.",
                status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        await _next(context);
    }

    private static bool IsPublicPath(PathString path)
        => path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
           || path.Equals("/", StringComparison.OrdinalIgnoreCase)
           || path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
}
