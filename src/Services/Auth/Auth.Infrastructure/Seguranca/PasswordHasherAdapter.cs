using Auth.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Seguranca;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string senha) => _hasher.HashPassword(new object(), senha);

    public bool Verificar(string senha, string senhaHash) =>
        _hasher.VerifyHashedPassword(new object(), senhaHash, senha) != PasswordVerificationResult.Failed;
}
