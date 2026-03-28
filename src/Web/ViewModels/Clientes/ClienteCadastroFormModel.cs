namespace Web.ViewModels.Clientes;

public sealed class ClienteCadastroFormModel
{
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
}
