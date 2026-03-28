namespace Web.ViewModels.Equipamentos;

public sealed class EquipamentoCadastroFormModel
{
    public string Tipo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? NumeroSerie { get; set; }
}
