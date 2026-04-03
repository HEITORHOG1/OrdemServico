using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OrdemServicoTaxaConfiguration : IEntityTypeConfiguration<OrdemServicoTaxa>
{
    public void Configure(EntityTypeBuilder<OrdemServicoTaxa> builder)
    {
        builder.ToTable("os_taxas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(150);

        builder.OwnsOne(x => x.Valor, v =>
        {
            v.Property(d => d.Valor).HasColumnName("valor").HasPrecision(18, 2);
        });

        builder.HasOne<OrdemServico>()
            .WithMany(o => o.Taxas)
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
