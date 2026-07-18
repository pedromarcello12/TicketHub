namespace Auth.Application.Auth.DTOs;

public record ResultadoAutenticacao(UsuarioResponse Usuario, string RefreshToken);
