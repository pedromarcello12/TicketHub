using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TicketHub.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarEmissorJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    public static IServiceCollection AdicionarAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var opcoes = new JwtOptions();
        configuration.GetSection(JwtOptions.SectionName).Bind(opcoes);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = opcoes.Issuer,
                    ValidAudience = opcoes.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcoes.SecretKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AdicionarClienteServicoInterno(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServicoInternoOptions>(configuration.GetSection(ServicoInternoOptions.SectionName));

        services.AddHttpClient(ServicoInternoConstantes.NomeHttpClient, (provedor, client) =>
        {
            var opcoes = provedor.GetRequiredService<IOptions<ServicoInternoOptions>>().Value;
            client.BaseAddress = new Uri(opcoes.AuthApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        services.AddSingleton<ServicoTokenProvider>();
        services.AddTransient<AuthTokenDelegatingHandler>();

        return services;
    }
}
