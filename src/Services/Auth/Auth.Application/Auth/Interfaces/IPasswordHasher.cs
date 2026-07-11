namespace Auth.Application.Auth.Interfaces;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string senhaHash);
}
