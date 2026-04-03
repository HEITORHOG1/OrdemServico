namespace Application.DTOs.Equipamentos;

public record EquipamentoResponse(Guid Id, Guid ClienteId, string Tipo, string? Marca, string? Modelo, string? NumeroSerie, DateTime CreatedAt);
