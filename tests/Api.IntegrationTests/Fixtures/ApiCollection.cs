namespace Api.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class ApiTestSuite : ICollectionFixture<WebApplicationFixture>
{
    public const string Name = "api-collection";
}
