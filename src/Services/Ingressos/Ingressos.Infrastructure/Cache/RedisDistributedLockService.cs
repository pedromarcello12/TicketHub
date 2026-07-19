using Ingressos.Application.Ingressos.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ingressos.Infrastructure.Cache;

public class RedisDistributedLockService(
    IConnectionMultiplexer conexao,
    ILogger<RedisDistributedLockService> logger) : IDistributedLockService
{
    private const int MaxTentativas = 5;
    private static readonly TimeSpan IntervaloEntreTentativas = TimeSpan.FromMilliseconds(200);

    public async Task<IAsyncDisposable?> AdquirirAsync(string chave, TimeSpan duracao, CancellationToken cancellationToken)
    {
        var db = conexao.GetDatabase();
        var token = Guid.NewGuid().ToString("N");

        for (var tentativa = 0; tentativa < MaxTentativas; tentativa++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var adquirido = await db.StringSetAsync(chave, token, duracao, When.NotExists);
            if (adquirido)
                return new RedisLockHandle(db, chave, token);

            await Task.Delay(IntervaloEntreTentativas, cancellationToken);
        }

        logger.LogWarning(
            "Nao foi possivel adquirir o lock distribuido para a chave {Chave} apos {Tentativas} tentativas",
            chave,
            MaxTentativas);

        return null;
    }

    private sealed class RedisLockHandle(IDatabase db, string chave, string token) : IAsyncDisposable
    {
        private const string ScriptLiberarSeDono = """
            if redis.call("get", KEYS[1]) == ARGV[1] then
                return redis.call("del", KEYS[1])
            else
                return 0
            end
            """;

        public async ValueTask DisposeAsync()
        {
            await db.ScriptEvaluateAsync(ScriptLiberarSeDono, [chave], [token]);
        }
    }
}
