using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record AlterarStatusRequestModel(
    [property: JsonPropertyName("novoStatus")] StatusOsModel NovoStatus,
    [property: JsonPropertyName("expectedUpdatedAt")] DateTime? ExpectedUpdatedAt = null);
