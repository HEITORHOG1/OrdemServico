using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrdemServicoProdutoConfiguration : IEntityTypeConfiguration<OrdemServicoProduto>
{
    public void Configure(EntityTypeBuilder<OrdemServicoProduto> builder)
    {
        builder.ToTable("os_produtos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(200);

        builder.OwnsOne(x => x.ValorUnitario, v =>
        {
            v.Property(d => d.Valor).HasColumnName("valor_unitario").HasPrecision(18, 2);
        });

        builder.HasOne<OrdemServico>()
            .WithMany(o => o.Produtos)
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
