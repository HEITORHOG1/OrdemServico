---
name: criar-entidade
description: Cria uma nova entidade de dominio seguindo os padroes DDD do projeto - com factory method, backing fields, guard clauses e excecoes de dominio.
disable-model-invocation: true
user-invocable: true
argument-hint: "[nome-da-entidade]"
---

# Criar Entidade de Dominio — $ARGUMENTS

Crie a entidade **$ARGUMENTS** em `src/Domain/Entities/` seguindo rigorosamente os padroes DDD do projeto.

## Estrutura Obrigatoria

```csharp
namespace Domain.Entities;

/// <summary>
/// [Descricao em portugues da entidade]
/// </summary>
public sealed class $ARGUMENTS
{
    public Guid Id { get; private set; }

    // Propriedades com private set
    // Strings required: = string.Empty
    // Navigation properties required do EF: = default!
    // Nullable: tipo?

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Colecoes filhas (se houver):
    // private readonly List<Filho> _filhos = new();
    // public IReadOnlyCollection<Filho> Filhos => _filhos.AsReadOnly();

    // Construtor privado para EF Core
    private $ARGUMENTS() { }

    // Factory method estatico
    public static $ARGUMENTS Criar(/* parametros */)
    {
        // Guard clauses com ArgumentException
        // if (string.IsNullOrWhiteSpace(nome))
        //     throw new ArgumentException("Nome e obrigatorio.", nameof(nome));

        var agora = DateTime.UtcNow;
        return new $ARGUMENTS
        {
            Id = Guid.NewGuid(),
            // ... propriedades
            CreatedAt = agora,
            UpdatedAt = agora
        };
    }

    // Metodos de comportamento (Rich Domain Model)
    // Sempre atualizar UpdatedAt no final
    // Usar DomainException para violacoes de regra de negocio
    // Usar ArgumentException para argumentos invalidos
}
```

## Checklist

- [ ] Classe `sealed`
- [ ] Construtor privado sem parametros para EF
- [ ] Factory method `Criar()` estatico
- [ ] `Id` como `Guid` com `private set`
- [ ] `CreatedAt` e `UpdatedAt` com `private set`
- [ ] Propriedades com `private set` (nunca public set)
- [ ] Strings required inicializadas com `string.Empty`
- [ ] Guard clauses nos metodos
- [ ] `UpdatedAt = DateTime.UtcNow` em todo metodo mutante
- [ ] Colecoes via backing field + IReadOnlyCollection
- [ ] Interface de repositorio em `Domain/Interfaces/`

Apos criar a entidade, pergunte ao usuario se deseja criar tambem a Configuration do EF Core, o repositorio e o service.
