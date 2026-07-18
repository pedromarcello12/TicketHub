using Auth.Application.Auth.DTOs;

namespace Auth.Application.Auth.Interfaces;

public interface IAuthAppService
{
    Task<ResultadoAutenticacao> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken);
    Task<ResultadoAutenticacao?> AutenticarAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<ResultadoAutenticacao?> RenovarTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
