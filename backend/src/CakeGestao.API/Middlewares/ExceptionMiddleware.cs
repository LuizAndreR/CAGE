using System.Net;
using System.Text.Json;

namespace CakeGestao.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationError vex)
        {
            _logger.LogInformation("Erro de validação: {Errors}", string.Join(", ", vex.Errors));

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var response = new
            {
                title = "Erro de Validação",
                status = context.Response.StatusCode,
                errors = vex.Errors,
                traceId = context.TraceIdentifier,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (NotFoundError nfex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            var response = new
            {
                title = "Recurso não encontrado",
                status = context.Response.StatusCode,
                message = nfex.Errors,
                traceId = context.TraceIdentifier
            };
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (ConflictError cfex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            var response = new
            {
                title = "Conflito de dados",
                status = context.Response.StatusCode,
                message = cfex.Errors,
                traceId = context.TraceIdentifier
            };
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Um erro inesperado ocorreu: {Message}", ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                title = "Erro Interno do Servidor",
                status = context.Response.StatusCode,
                error = "Ocorreu um erro interno no servidor. Tente novamente mais tarde.",
                traceId = context.TraceIdentifier,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
