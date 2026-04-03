using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
#pragma warning disable CA1848
        _logger.LogError(exception, "Ocorreu uma exceção: {Message}", exception.Message);
#pragma warning restore CA1848

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Detail = exception.Message
        };

        problemDetails.Extensions["correlationId"] = httpContext.Response.Headers[CorrelationIdHeader].ToString();

        if (exception is ConcurrencyConflictException)
        {
            problemDetails.Status = StatusCodes.Status409Conflict;
            problemDetails.Title = "Conflito de concorrencia";
        }
        else if (exception is DomainException or ArgumentException)
        {
            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
            problemDetails.Title = "Erro de Regra de Negócio (Domain)";
        }
        else if (exception is KeyNotFoundException) // Caso seja retornado do BD
        {
            problemDetails.Status = StatusCodes.Status404NotFound;
            problemDetails.Title = "Recurso não encontrado";
        }
        else
        {
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Erro interno no servidor";
            // Em produção não deve vazar exception.Message, mas como isso é ex. didático, OK.
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
