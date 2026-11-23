using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UsuariosApi.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is ExcecaoDeDominio dominio)
        {
            context.Response.StatusCode = dominio.StatusCode;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = dominio.StatusCode,
                Title = dominio.Titulo,
                Detail = dominio.Message
            }, ct);
            return true;
        }

        logger.LogError(exception, "Erro não tratado");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno",
            Detail = "Ocorreu um erro inesperado."
        }, ct);
        return true;
    }
}
