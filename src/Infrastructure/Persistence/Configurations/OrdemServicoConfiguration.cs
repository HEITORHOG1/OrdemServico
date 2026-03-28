using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("ordens_servico");
        
        builder.HasKey(x => x.Id);

        // Value Object - NumeroOS manipulado como ComplexType sem tabela separada
        builder.OwnsOne(x => x.Numero, numero =>
        {
            numero.Property(n => n.Valor)
                  .HasColumnName("numero")
                  .HasMaxLength(20)
                  .IsRequired();
            numero.HasIndex(n => n.Valor).IsUnique();
        });

        // Value Object - DescontoAplicado (pode ser null)
        builder.OwnsOne(x => x.DescontoAplicado, desconto =>
        {
            desconto.Property(d => d.Tipo).HasColumnName("desconto_tipo");
            desconto.Property(d => d.Valor).HasColumnName("desconto_valor").HasPrecision(18, 2);
        });

        builder.Property(x => x.Defeito)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.LaudoTecnico)
            .HasColumnType("text");

        builder.Property(x => x.Observacoes)
            .HasColumnType("text");
            
        builder.Property(x => x.CondicoesPagamento)
            .HasMaxLength(250);

        builder.Property(x => x.Referencia)
            .HasMaxLength(100);

        builder.Property(x => x.Duracao)
            .HasMaxLength(50);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipamento>()
            .WithMany()
            .HasForeignKey(x => x.EquipamentoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configurando coleções com Backing Fields (clean architecture approach)
        builder.Metadata.FindNavigation(nameof(OrdemServico.Servicos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            
        builder.Metadata.FindNavigation(nameof(OrdemServico.Produtos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(OrdemServico.Taxas))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(OrdemServico.Pagamentos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(OrdemServico.Fotos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(OrdemServico.Anotacoes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
