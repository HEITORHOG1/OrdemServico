using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OrdemServicoFotoConfiguration : IEntityTypeConfiguration<OrdemServicoFoto>
{
    public void Configure(EntityTypeBuilder<OrdemServicoFoto> builder)
    {
        builder.ToTable("os_fotos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Legenda).HasMaxLength(250);

        builder.HasOne<OrdemServico>()
            .WithMany(o => o.Fotos)
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
