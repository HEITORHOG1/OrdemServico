using Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default)
    {
        var valor = await _db.StringGetAsync(chave);
        if (valor.IsNull)
            return default;

        return JsonSerializer.Deserialize<T>(valor!);
    }

    public async Task DefinirAsync<T>(string chave, T valor, TimeSpan? tempoExpiracao = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(valor);
        if (tempoExpiracao.HasValue)
        {
            await _db.StringSetAsync(chave, json, tempoExpiracao.Value);
        }
        else
        {
            await _db.StringSetAsync(chave, json);
        }
    }

    public async Task RemoverAsync(string chave, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(chave);
    }
}
