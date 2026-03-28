namespace Web.Services.Api;

public sealed class ApiSettings
{
    public const string SectionName = "Api";

    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string? ApiKey { get; set; }
}
