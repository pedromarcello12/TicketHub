using System.Net;
using Ingressos.Application.Ingressos.Interfaces;
using Polly.CircuitBreaker;
using Polly.Timeout;
using TicketHub.Core.Excecoes;

namespace Ingressos.Infrastructure.ServicosExternos;

public class HttpEventoExternalService(HttpClient httpClient) : IEventoExternalService
{
    public async Task<bool> ExisteAsync(Guid eventoId, CancellationToken cancellationToken)
    {
        try
        {
            using var resposta = await httpClient.GetAsync($"/api/Eventos/{eventoId}", cancellationToken);

            if (resposta.StatusCode == HttpStatusCode.NotFound)
                return false;

            if (!resposta.IsSuccessStatusCode)
                throw new ServicoExternoIndisponivelException(
                    $"Servico de Eventos retornou {(int)resposta.StatusCode} ao consultar o evento '{eventoId}'.");

            return true;
        }
        catch (HttpRequestException ex)
        {
            throw new ServicoExternoIndisponivelException("Servico de Eventos esta indisponivel.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ServicoExternoIndisponivelException("Servico de Eventos nao respondeu a tempo.", ex);
        }
        catch (TimeoutRejectedException ex)
        {
            throw new ServicoExternoIndisponivelException("Servico de Eventos nao respondeu a tempo.", ex);
        }
        catch (BrokenCircuitException ex)
        {
            throw new ServicoExternoIndisponivelException("Servico de Eventos esta indisponivel (circuito aberto).", ex);
        }
    }
}
