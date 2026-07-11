using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TicketHub.Core.Excecoes;

namespace Pagamento.Api.Filtros;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = context.Exception switch
        {
            InvalidOperationException ex => new ConflictObjectResult(new { mensagem = ex.Message }),
            RecursoRelacionadoNaoEncontradoException ex => new BadRequestObjectResult(new { mensagem = ex.Message }),
            ServicoExternoIndisponivelException ex => new ObjectResult(new { mensagem = ex.Message }) { StatusCode = StatusCodes.Status503ServiceUnavailable },
            _ => context.Result
        };

        if (context.Result is not null)
            context.ExceptionHandled = true;
    }
}
