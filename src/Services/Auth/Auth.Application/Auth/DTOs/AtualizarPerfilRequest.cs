namespace Auth.Application.Auth.DTOs;

public record AtualizarPerfilRequest(string? Nome, string? SenhaAtual, string? NovaSenha);
