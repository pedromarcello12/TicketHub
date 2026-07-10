using Ingressos.Application.Ingressos.DTOs;

namespace Ingressos.Application.Ingressos.Interfaces;

public interface IIngressoAppService
{
    Task<IngressoResponse> CriarAsync(CriarIngressoRequest request, CancellationToken cancellationToken);
    Task<IngressoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
}
