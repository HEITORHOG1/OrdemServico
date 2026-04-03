using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class OrdemServicoPagamentoConfiguration : IEntityTypeConfiguration<OrdemServicoPagamento>
{
    public void Configure(EntityTypeBuilder<OrdemServicoPagamento> builder)
    {
        builder.ToTable("os_pagamentos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MeioPagamento)
            .IsRequired()
            .HasConversion<string>() // grava ENUM como string!
            .HasMaxLength(50);

        builder.OwnsOne(x => x.Valor, v =>
        {
            v.Property(d => d.Valor).HasColumnName("valor").HasPrecision(18, 2);
        });

        builder.HasOne<OrdemServico>()
            .WithMany(o => o.Pagamentos)
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
