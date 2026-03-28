using Api.Extensions;
using Serilog;
using System.Globalization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApiSetup(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TenantId",
                httpContext.User?.FindFirst("tenant_id")?.Value ?? "anonymous");
            diagnosticContext.Set("UsuarioId",
                httpContext.User?.FindFirst("usuario_id")?.Value ?? "anonymous");

            var correlationId = httpContext.Items.TryGetValue("CorrelationId", out var cid)
                ? cid?.ToString()
                : httpContext.Response.Headers["X-Correlation-Id"].ToString();

            if (!string.IsNullOrWhiteSpace(correlationId))
                diagnosticContext.Set("CorrelationId", correlationId);
        };
    });

    app.UseApiSetup();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicacao encerrou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
