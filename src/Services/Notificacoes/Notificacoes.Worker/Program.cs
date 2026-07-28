using Notificacoes.Worker.Email;
using Notificacoes.Worker.Workers;
using TicketHub.MessageBus;
using TicketHub.Observabilidade;

var builder = Host.CreateApplicationBuilder(args);

builder.AdicionarObservabilidadeWorker("notificacoes-worker");

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// MassTransit — registra consumer com sua definição (endpoint name + retry)
builder.Services.AdicionarMassTransitRabbitMq(builder.Configuration, bus =>
    bus.AddConsumer<PagamentoStatusAlteradoConsumer, PagamentoStatusAlteradoConsumerDefinition>());

var host = builder.Build();
host.Run();
