using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OrdemServicoAnotacaoConfiguration : IEntityTypeConfiguration<OrdemServicoAnotacao>
{
    public void Configure(EntityTypeBuilder<OrdemServicoAnotacao> builder)
    {
        builder.ToTable("os_anotacoes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Texto).IsRequired().HasColumnType("text");
        builder.Property(x => x.Autor).IsRequired().HasMaxLength(100);

        builder.HasOne<OrdemServico>()
            .WithMany(o => o.Anotacoes)
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
