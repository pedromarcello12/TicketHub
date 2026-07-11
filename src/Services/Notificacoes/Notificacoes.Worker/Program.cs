using Notificacoes.Worker.Workers;
using TicketHub.MessageBus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHostedService<PagamentoStatusAlteradoConsumer>();

var host = builder.Build();
host.Run();
