namespace Auth.Api.Usuarios;

public static class UsuariosSeed
{
    public static readonly IReadOnlyList<Usuario> Usuarios =
    [
        new Usuario("1", "admin", "admin123", "Administrador TicketHub", "Administrador"),
        new Usuario("2", "cliente", "cliente123", "Cliente TicketHub", "Cliente")
    ];

    public static Usuario? Autenticar(string nomeUsuario, string senha) =>
        Usuarios.FirstOrDefault(u =>
            u.NomeUsuario.Equals(nomeUsuario, StringComparison.OrdinalIgnoreCase) &&
            u.Senha == senha);
}
