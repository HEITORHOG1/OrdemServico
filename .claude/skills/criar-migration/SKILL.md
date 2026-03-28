---
name: criar-migration
description: Cria uma migration do EF Core seguindo os padroes do projeto - com Configuration separada, snake_case, OwnsOne para value objects e backing fields.
disable-model-invocation: true
user-invocable: true
argument-hint: "[nome-da-migration]"
---

# Criar Migration EF Core — $ARGUMENTS

Crie a migration **$ARGUMENTS** seguindo os padroes de persistencia do projeto.

## 1. Verificar/Criar Configuration

Se a entidade ainda nao tem configuracao, criar em `src/Infrastructure/Persistence/Configurations/`:

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class XxxConfiguration : IEntityTypeConfiguration<Xxx>
{
    public void Configure(EntityTypeBuilder<Xxx> builder)
    {
        builder.ToTable("nome_tabela_snake_case");

        builder.HasKey(x => x.Id);

        // Value Objects via OwnsOne
        builder.OwnsOne(x => x.ValueObject, vo =>
        {
            vo.Property(v => v.Propriedade)
              .HasColumnName("coluna_snake_case")
              .HasMaxLength(100)
              .IsRequired();
        });

        // Strings
        builder.Property(x => x.Campo)
            .IsRequired()
            .HasMaxLength(200);

        // Texto longo
        builder.Property(x => x.Descricao)
            .HasColumnType("text");

        // Decimal com precisao
        builder.Property(x => x.Valor)
            .HasPrecision(18, 2);

        // Relacionamentos
        builder.HasOne<OutraEntidade>()
            .WithMany()
            .HasForeignKey(x => x.OutraEntidadeId)
            .OnDelete(DeleteBehavior.Restrict);  // Restrict para required, SetNull para optional

        // Colecoes com backing field
        builder.Metadata.FindNavigation(nameof(Xxx.Filhos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

## 2. Adicionar DbSet no AppDbContext

Em `src/Infrastructure/Persistence/AppDbContext.cs`:
```csharp
public DbSet<Xxx> NomePlural { get; set; }
```

## 3. Gerar Migration

```bash
dotnet ef migrations add $ARGUMENTS --project src/Infrastructure --startup-project src/Api
```

## 4. Aplicar Migration

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

## Convencoes de Banco

| C# | MySQL |
|----|-------|
| Tabela | snake_case plural: `ordens_servico`, `clientes` |
| Coluna | snake_case: `desconto_tipo`, `created_at` |
| PK | CHAR(36) GUID |
| FK required | DeleteBehavior.Restrict |
| FK optional | DeleteBehavior.SetNull |
| string curta | HasMaxLength(N) |
| string longa | HasColumnType("text") |
| decimal | HasPrecision(18, 2) |
| Value Object | OwnsOne() mapeado como colunas |
| Colecao | Backing field + PropertyAccessMode.Field |
| Index unico | HasIndex().IsUnique() |
