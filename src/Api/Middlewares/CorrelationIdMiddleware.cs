namespace Api.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";
    public const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            // Evitar erro se o request the mock não suportar read-only headers collection,
            // Normalmente nós adicionariamos, mas pra minimal API simples vamos so processar a resposta.
        }

        context.Items[CorrelationIdItemKey] = correlationId.ToString();

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // Adiciona um escopo de log (requer configuração no Serilog/AppInsights para printar scopes)
        // using (logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))

        await _next(context);
    }
}
