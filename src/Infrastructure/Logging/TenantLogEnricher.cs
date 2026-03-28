using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging;

/// <summary>
/// Enricher do Serilog que injeta TenantId e UsuarioId em todos os log events.
/// Extrai as claims do JWT do usuario autenticado.
/// </summary>
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
