using System.Net;
using System.Text.Json;
using WariSalud.Core.Exceptions;

namespace WariSalud.API.Middleware;

/// <summary>
/// Middleware global que captura todas las excepciones de dominio y las
/// traduce a respuestas HTTP consistentes (RNF05, plan.md §6).
/// Evita que cualquier excepción no controlada exponga un stack trace 500 genérico.
/// T4.5
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, mensaje) = exception switch
        {
            DoubleBookingException e       => (HttpStatusCode.Conflict, e.Message),
            LimiteDeCitasException e       => (HttpStatusCode.Conflict, e.Message),
            FueraDeHorarioException e      => (HttpStatusCode.UnprocessableEntity, e.Message),
            CancelacionFueraDePlazoException e => (HttpStatusCode.UnprocessableEntity, e.Message),
            RecursoNoEncontradoException e => (HttpStatusCode.NotFound, e.Message),
            AccesoNoAutorizadoException e  => (HttpStatusCode.Forbidden, e.Message),
            UnauthorizedAccessException e  => (HttpStatusCode.Unauthorized, e.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error interno en el servidor.")
        };

        // Log siempre — incluyendo el stack trace para el 500 genérico
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
        else
            _logger.LogWarning("Excepción de dominio [{Type}]: {Message}", exception.GetType().Name, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var respuesta = new
        {
            error = mensaje,
            tipo = exception.GetType().Name,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(respuesta, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
