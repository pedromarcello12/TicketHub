using Auth.Application.Auth;
using Auth.Application.Auth.Interfaces;
using Auth.Application.Auth.Servicos;
using Auth.Infrastructure.Persistencia;
using Auth.Infrastructure.Repositorios;
using Auth.Infrastructure.Seguranca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AuthDb")));

        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));

        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IRefreshTokenRepositorio, RefreshTokenRepositorio>();
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IAuthAppService, AuthAppService>();

        return services;
    }
}
