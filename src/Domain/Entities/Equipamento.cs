namespace Domain.Entities;

/// <summary>
/// Equipamento registrado para o cliente (mantém o histórico do equipamento sob serviços do cliente).
/// </summary>
public sealed class Equipamento
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }

    public string Tipo { get; private set; } = string.Empty; // Ex: Celular, Notebook, Geladeira
    public string? Marca { get; private set; }
    public string? Modelo { get; private set; }
    public string? NumeroSerie { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Equipamento() { }

    public static Equipamento Criar(Guid clienteId, string tipo, string? marca, string? modelo, string? numeroSerie)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("O equipamento deve pertencer a um cliente.", nameof(clienteId));

        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("O tipo do equipamento é obrigatório.", nameof(tipo));

        var dataAtual = DateTime.UtcNow;

        return new Equipamento
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Tipo = tipo,
            Marca = marca,
            Modelo = modelo,
            NumeroSerie = numeroSerie,
            CreatedAt = dataAtual,
            UpdatedAt = dataAtual
        };
    }

    public void Atualizar(string tipo, string? marca, string? modelo, string? numeroSerie)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("O tipo do equipamento é obrigatório.", nameof(tipo));

        Tipo = tipo;
        Marca = marca;
        Modelo = modelo;
        NumeroSerie = numeroSerie;
        UpdatedAt = DateTime.UtcNow;
    }
}
