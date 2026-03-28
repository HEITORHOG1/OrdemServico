namespace Infrastructure.Caching;

public static class CacheKeys
{
    public static string Cliente(Guid id) => $"cliente:{id}";
    public static string OS(string numero) => $"os:{numero}";
}
