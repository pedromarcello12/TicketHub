using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Notificacoes.Worker.Email;

public class SmtpEmailSender(IOptions<EmailOptions> opcoes) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken cancellationToken)
    {
        var configuracao = opcoes.Value;

        using var client = new SmtpClient(configuracao.SmtpHost, configuracao.SmtpPort)
        {
            EnableSsl = configuracao.UsarSsl
        };

        if (!string.IsNullOrWhiteSpace(configuracao.UserName))
            client.Credentials = new NetworkCredential(configuracao.UserName, configuracao.Password);

        using var mensagem = new MailMessage(configuracao.Remetente, destinatario, assunto, corpo);

        await client.SendMailAsync(mensagem, cancellationToken);
    }
}
