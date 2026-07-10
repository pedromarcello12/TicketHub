using Ingressos.Application.Ingressos.Interfaces;
using Ingressos.Application.Ingressos.Servicos;
using Ingressos.Infrastructure.Persistencia;
using Ingressos.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ingressos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructureIngressos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IngressosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IngressosDb")));

        services.AddScoped<IIngressoRepositorio, IngressoRepositorio>();
        services.AddScoped<IIngressoAppService, IngressoAppService>();

        return services;
    }
}
