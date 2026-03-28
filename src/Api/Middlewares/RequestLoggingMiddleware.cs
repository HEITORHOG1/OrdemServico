using System.Diagnostics;

namespace Api.Middlewares;

public sealed partial class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemKey, out var correlationObj)
                ? correlationObj?.ToString()
                : context.Request.Headers[CorrelationIdHeader].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = context.Response.Headers[CorrelationIdHeader].ToString();
            }

            LogRequestCompleted(
                _logger,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP {Method} {Path} respondido {StatusCode} em {ElapsedMilliseconds}ms CorrelationId={CorrelationId}")]
    private static partial void LogRequestCompleted(ILogger logger, string method, PathString path, int statusCode, long elapsedMilliseconds, string? correlationId);
}
