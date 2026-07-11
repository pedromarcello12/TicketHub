using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TicketHub.MessageBus.Eventos;

namespace TicketHub.MessageBus;

public abstract class RabbitMqConsumerBackgroundService<TEvento> : BackgroundService where TEvento : IntegrationEvent
{
    private readonly RabbitMqOptions _opcoes;
    private readonly ILogger _logger;
    private readonly string _fila;
    private readonly string _routingKey;
    private IConnection? _conexao;
    private IModel? _canal;

    protected RabbitMqConsumerBackgroundService(
        IOptions<RabbitMqOptions> opcoes,
        ILogger logger,
        string fila,
        string routingKey)
    {
        _opcoes = opcoes.Value;
        _logger = logger;
        _fila = fila;
        _routingKey = routingKey;
    }

    protected abstract Task TratarAsync(TEvento evento, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _opcoes.HostName,
            Port = _opcoes.Port,
            UserName = _opcoes.UserName,
            Password = _opcoes.Password
        };

        _conexao = factory.CreateConnection();
        _canal = _conexao.CreateModel();

        _canal.ExchangeDeclare(RabbitMqConstantes.ExchangeEventos, ExchangeType.Topic, durable: true);
        _canal.QueueDeclare(_fila, durable: true, exclusive: false, autoDelete: false);
        _canal.QueueBind(_fila, RabbitMqConstantes.ExchangeEventos, _routingKey);
        _canal.BasicQos(0, 1, false);

        var consumidor = new EventingBasicConsumer(_canal);
        consumidor.Received += async (_, ea) =>
        {
            try
            {
                var evento = JsonSerializer.Deserialize<TEvento>(ea.Body.Span);
                if (evento is not null)
                    await TratarAsync(evento, stoppingToken);

                _canal.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar mensagem da fila {Fila}", _fila);
                _canal.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _canal.BasicConsume(_fila, autoAck: false, consumidor);

        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() => tcs.TrySetResult());
        await tcs.Task;
    }

    public override void Dispose()
    {
        _canal?.Dispose();
        _conexao?.Dispose();
        base.Dispose();
    }
}
