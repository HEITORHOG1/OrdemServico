using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record PagedResponseModel<T>(
    [property: JsonPropertyName("items")] IReadOnlyCollection<T> Items,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages);
