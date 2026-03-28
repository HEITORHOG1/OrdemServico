namespace Application.DTOs.Equipamentos;

public record CriarEquipamentoRequest(Guid ClienteId, string Tipo, string? Marca, string? Modelo, string? NumeroSerie);

public record EquipamentoResponse(Guid Id, Guid ClienteId, string Tipo, string? Marca, string? Modelo, string? NumeroSerie, DateTime CreatedAt);
