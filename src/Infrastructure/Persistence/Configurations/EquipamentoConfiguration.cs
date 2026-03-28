using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EquipamentoConfiguration : IEntityTypeConfiguration<Equipamento>
{
    public void Configure(EntityTypeBuilder<Equipamento> builder)
    {
        builder.ToTable("equipamentos");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.Marca)
            .HasMaxLength(50);
            
        builder.Property(x => x.Modelo)
            .HasMaxLength(50);
            
        builder.Property(x => x.NumeroSerie)
            .HasMaxLength(100);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
