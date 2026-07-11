using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pagamento.Application.Pagamentos.Interfaces;
using Pagamento.Application.Pagamentos.Servicos;
using Pagamento.Infrastructure.Persistencia;
using Pagamento.Infrastructure.Repositorios;

namespace Pagamento.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfrastructurePagamento(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PagamentosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("PagamentoDb")));

        services.AddScoped<IPagamentoRepositorio, PagamentoRepositorio>();
        services.AddScoped<IPagamentoAppService, PagamentoAppService>();

        return services;
    }
}
