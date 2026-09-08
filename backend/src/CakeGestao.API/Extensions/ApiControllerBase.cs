using CakeGestao.Domain.Exceptions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result, ILogger? logger = null, string prefix = "")
    {
        var traceId = HttpContext.TraceIdentifier;

        if (logger != null)
        {
            if (result.IsSuccess)
            {
                // Se quiser ser muito detalhista, pode logar o valor de retorno também, mas cuidado com dados sensíveis
                logger.LogInformation("{LogPrefix} Operação concluída com sucesso. TraceId: {TraceId}", prefix, traceId);
            }
            else
            {
                // Aqui capturamos os erros reais que vieram do Handler
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Message));
                logger.LogWarning("{LogPrefix} Falha na operação. Erros: {Errors} | TraceId: {TraceId}", prefix, errorMessages, traceId);
            }
        }

        if (result.IsSuccess)
            return Ok(result.Value);

        var error = result.Errors.FirstOrDefault();
        if (error is ValidationError validationError)
        {
            return BadRequest(new
            {
                title = "Erro de Validação",
                status = 400,
                errors = validationError.Errors,
                traceId
            });
        }
        if (error is ConflictError conflictError)
        {
            return Conflict(new
            {
                title = "Conflito",
                status = 409,
                error = conflictError.Errors,
                traceId
            });
        }
        if (error is NotFoundError notFoundError)
        {
            return NotFound(new
            {
                title = "Não Encontrado",
                status = 404,
                error = notFoundError.Errors,
                traceId
            });
        }

        return StatusCode(500, new
        {
            title = "Erro Interno do Servidor",
            status = 500,
            error = error?.Message ?? "Ocorreu um erro interno inesperado.",
            traceId
        });
    }
}
