using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        
        var connectionString = "Server=localhost;Database=os_db;Uid=root;Pwd=root;";

        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AppDbContext(builder.Options);
    }
}
