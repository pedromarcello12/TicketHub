namespace Notificacoes.Worker.Email;

public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken cancellationToken);
}
