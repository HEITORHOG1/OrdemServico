using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record AdicionarAnotacaoRequestModel(
    [property: JsonPropertyName("texto")] string Texto,
    [property: JsonPropertyName("autor")] string Autor);
