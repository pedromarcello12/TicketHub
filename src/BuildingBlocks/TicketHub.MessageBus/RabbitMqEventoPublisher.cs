using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using TicketHub.MessageBus.Eventos;

namespace TicketHub.MessageBus;

public class RabbitMqEventoPublisher : IEventoPublisher, IDisposable
{
    private readonly IConnection _conexao;
    private readonly IModel _canal;
    private readonly ISyncPolicy _politicaPublicacao;

    public RabbitMqEventoPublisher(IOptions<RabbitMqOptions> opcoes, ILogger<RabbitMqEventoPublisher> logger)
    {
        var configuracao = opcoes.Value;
        var factory = new ConnectionFactory
        {
            HostName = configuracao.HostName,
            Port = configuracao.Port,
            UserName = configuracao.UserName,
            Password = configuracao.Password
        };

        var politicaConexao = RabbitMqRetryPolicies.CriarPoliticaConexao(logger);

        _conexao = politicaConexao.Execute(() => factory.CreateConnection());
        _canal = _conexao.CreateModel();
        _canal.ExchangeDeclare(RabbitMqConstantes.ExchangeEventos, ExchangeType.Topic, durable: true);

        _politicaPublicacao = RabbitMqRetryPolicies.CriarPoliticaPublicacao(logger);
    }

    public void Publicar<TEvento>(TEvento evento, string routingKey) where TEvento : IntegrationEvent
    {
        _politicaPublicacao.Execute(() =>
        {
            var corpo = JsonSerializer.SerializeToUtf8Bytes(evento);

            var propriedades = _canal.CreateBasicProperties();
            propriedades.Persistent = true;
            propriedades.ContentType = "application/json";

            _canal.BasicPublish(RabbitMqConstantes.ExchangeEventos, routingKey, propriedades, corpo);
        });
    }

    public void Dispose()
    {
        _canal.Dispose();
        _conexao.Dispose();
        GC.SuppressFinalize(this);
    }
}
