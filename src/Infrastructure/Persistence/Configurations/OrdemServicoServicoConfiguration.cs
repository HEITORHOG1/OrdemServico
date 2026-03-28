using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrdemServicoServicoConfiguration : IEntityTypeConfiguration<OrdemServicoServico>
{
    public void Configure(EntityTypeBuilder<OrdemServicoServico> builder)
    {
        builder.ToTable("os_servicos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(200);

        builder.OwnsOne(x => x.ValorUnitario, v =>
        {
            v.Property(d => d.Valor).HasColumnName("valor_unitario").HasPrecision(18, 2);
        });

        // Relacionamento master/detail
        builder.HasOne<OrdemServico>()
            .WithMany(o => o.Servicos)
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
