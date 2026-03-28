using Api.Endpoints;
using Api.Middlewares;
using Infrastructure.Persistence;

namespace Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiSetup(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Cria as tabelas do MySQL baseando-se nas Entities e Configurations sem precisar de Migrations.
            dbContext.Database.EnsureCreated();

            // Seed de dados iniciais (so insere se o banco estiver vazio)
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            seeder.SeedAsync().GetAwaiter().GetResult();
        }

        app.UseExceptionHandler(); // Usará o IExceptionHandler global via DI

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Custom Middlewares
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseCors("ApiCorsPolicy");
        app.UseMiddleware<ApiKeyAuthMiddleware>();

        app.UseHttpsRedirection();

        // Endpoints genéricos (minimal APIs root extension methods)
        app.MapClienteEndpoints();
        app.MapEquipamentoEndpoints();
        app.MapOrdemServicoEndpoints();

        // Endpoint de seed para desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.MapPost("/api/seed", async (DatabaseSeeder seeder, CancellationToken ct) =>
            {
                await seeder.ForceSeedAsync(ct);
                return Results.Ok(new { message = "Seed executado com sucesso." });
            }).WithTags("Dev").ExcludeFromDescription();
        }

        return app;
    }
}
