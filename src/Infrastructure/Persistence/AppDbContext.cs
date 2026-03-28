using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppIdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<OrdemServicoServico> OrdensServicoServicos => Set<OrdemServicoServico>();
    public DbSet<OrdemServicoProduto> OrdensServicoProdutos => Set<OrdemServicoProduto>();
    public DbSet<OrdemServicoTaxa> OrdensServicoTaxas => Set<OrdemServicoTaxa>();
    public DbSet<OrdemServicoPagamento> OrdensServicoPagamentos => Set<OrdemServicoPagamento>();
    public DbSet<OrdemServicoFoto> OrdensServicoFotos => Set<OrdemServicoFoto>();
    public DbSet<OrdemServicoAnotacao> OrdensServicoAnotacoes => Set<OrdemServicoAnotacao>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tables DEVEM ser configuradas PRIMEIRO
        base.OnModelCreating(builder);

        // Depois aplica as configurations customizadas (entidades de dominio)
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
