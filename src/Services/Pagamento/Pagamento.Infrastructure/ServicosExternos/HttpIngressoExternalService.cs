using System.Net;
using Pagamento.Application.Pagamentos.Interfaces;
using Polly.CircuitBreaker;
using Polly.Timeout;
using TicketHub.Core.Excecoes;

namespace Pagamento.Infrastructure.ServicosExternos;

public class HttpIngressoExternalService(HttpClient httpClient) : IIngressoExternalService
{
    public async Task<bool> ExisteAsync(Guid ingressoId, CancellationToken cancellationToken)
    {
        try
        {
            using var resposta = await httpClient.GetAsync($"/api/Ingressos/{ingressoId}", cancellationToken);

            if (resposta.StatusCode == HttpStatusCode.NotFound)
                return false;

            if (!resposta.IsSuccessStatusCode)
                throw new ServicoExternoIndisponivelException(
                    $"Servico de Ingressos retornou {(int)resposta.StatusCode} ao consultar o ingresso '{ingressoId}'.");

            return true;
        }
        catch (HttpRequestException ex)
        {
            throw new ServicoExternoIndisponivelException("Servico de Ingressos esta indisponivel.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ServicoExternoIndisponivelException("Servico de Ingressos nao respondeu a tempo.", ex);
        }
        catch (TimeoutRejectedException ex)
        {
            throw new ServicoExternoIndisponivelException("Servico de Ingressos nao respondeu a tempo.", ex);
        }
        catch (BrokenCircuitException ex)
        {
            throw new ServicoExternoIndisponivelException("Servico de Ingressos esta indisponivel (circuito aberto).", ex);
        }
    }
}
