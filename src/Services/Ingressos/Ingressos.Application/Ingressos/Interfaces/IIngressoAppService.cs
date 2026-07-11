using Ingressos.Application.Ingressos.DTOs;

namespace Ingressos.Application.Ingressos.Interfaces;

public interface IIngressoAppService
{
    Task<IngressoResponse> CriarAsync(CriarIngressoRequest request, CancellationToken cancellationToken);
    Task<IngressoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<IngressoResponse>> ListarAsync(Guid? eventoId, CancellationToken cancellationToken);
    Task<IngressoResponse?> ReservarAsync(Guid id, CancellationToken cancellationToken);
    Task<IngressoResponse?> ConfirmarVendaAsync(Guid id, CancellationToken cancellationToken);
    Task<IngressoResponse?> CancelarAsync(Guid id, CancellationToken cancellationToken);
}
