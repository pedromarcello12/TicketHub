using Auth.Api.Usuarios;
using FluentAssertions;
using Xunit;

namespace Auth.Tests;

public class UsuariosSeedTests
{
    [Fact]
    public void Autenticar_ComCredenciaisValidas_DeveRetornarUsuario()
    {
        var usuario = UsuariosSeed.Autenticar("admin", "admin123");

        usuario.Should().NotBeNull();
        usuario!.Papel.Should().Be("Administrador");
    }

    [Fact]
    public void Autenticar_ComSenhaInvalida_DeveRetornarNulo()
    {
        var usuario = UsuariosSeed.Autenticar("admin", "senha-errada");

        usuario.Should().BeNull();
    }

    [Fact]
    public void Autenticar_ComUsuarioInexistente_DeveRetornarNulo()
    {
        var usuario = UsuariosSeed.Autenticar("nao-existe", "qualquer");

        usuario.Should().BeNull();
    }

    [Fact]
    public void Autenticar_NomeUsuarioCaseInsensitive_DeveAutenticar()
    {
        var usuario = UsuariosSeed.Autenticar("ADMIN", "admin123");

        usuario.Should().NotBeNull();
    }
}
