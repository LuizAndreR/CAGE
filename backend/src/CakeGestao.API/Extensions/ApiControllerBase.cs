using FluentResults;
using Microsoft.AspNetCore.Mvc;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        var traceId = HttpContext.TraceIdentifier;

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
                error = conflictError.Message,
                traceId
            });
        }
        if (error is NotFoundError notFoundError)
        {
            return NotFound(new
            {
                title = "Não Encontrado",
                status = 404,
                error = notFoundError.Message,
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
