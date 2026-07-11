using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth.Api.Filtros;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = context.Exception switch
        {
            InvalidOperationException ex => new ConflictObjectResult(new { mensagem = ex.Message }),
            _ => context.Result
        };

        if (context.Result is not null)
            context.ExceptionHandled = true;
    }
}
