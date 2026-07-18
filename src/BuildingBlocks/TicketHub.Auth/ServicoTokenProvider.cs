using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TicketHub.Auth;

public class ServicoTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<ServicoInternoOptions> opcoes,
    ILogger<ServicoTokenProvider> logger)
{
    private readonly SemaphoreSlim _semaforo = new(1, 1);
    private string? _tokenCache;
    private DateTime _expiraEm = DateTime.MinValue;

    public async Task<string> ObterTokenAsync(CancellationToken cancellationToken)
    {
        if (TokenValido())
            return _tokenCache!;

        await _semaforo.WaitAsync(cancellationToken);
        try
        {
            if (TokenValido())
                return _tokenCache!;

            var configuracao = opcoes.Value;
            var client = httpClientFactory.CreateClient(ServicoInternoConstantes.NomeHttpClient);

            var resposta = await client.PostAsJsonAsync(
                "/api/auth/login",
                new { NomeUsuario = configuracao.NomeUsuario, Senha = configuracao.Senha },
                cancellationToken);

            resposta.EnsureSuccessStatusCode();

            var corpo = await resposta.Content.ReadFromJsonAsync<LoginServicoResponse>(cancellationToken);
            if (corpo is null)
                throw new InvalidOperationException("Auth.Api retornou resposta vazia ao autenticar o servico interno.");

            _tokenCache = corpo.Token;
            _expiraEm = corpo.ExpiraEm;

            logger.LogInformation("Token de servico interno renovado, expira em {ExpiraEm}.", _expiraEm);

            return _tokenCache;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private bool TokenValido() =>
        _tokenCache is not null && DateTime.UtcNow < _expiraEm - TimeSpan.FromMinutes(1);

    private record LoginServicoResponse(string Token, string Nome, string Papel, DateTime ExpiraEm);
}
