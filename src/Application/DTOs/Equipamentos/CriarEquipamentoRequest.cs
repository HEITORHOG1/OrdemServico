namespace Application.DTOs.Equipamentos;

public record CriarEquipamentoRequest(Guid ClienteId, string Tipo, string? Marca, string? Modelo, string? NumeroSerie);
