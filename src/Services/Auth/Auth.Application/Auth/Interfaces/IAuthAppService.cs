using Auth.Application.Auth.DTOs;

namespace Auth.Application.Auth.Interfaces;

public interface IAuthAppService
{
    Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken);
    Task<UsuarioResponse?> AutenticarAsync(LoginRequest request, CancellationToken cancellationToken);
}
