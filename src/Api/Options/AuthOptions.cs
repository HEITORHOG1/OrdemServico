namespace Api.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string? ApiKey { get; set; }

    public bool IsApiKeyRequired => !string.IsNullOrWhiteSpace(ApiKey);
}
