
using System.Net;
using api_csharp.DTOs;
using FluentValidation;

namespace api_csharp.Middleware;

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
        var errorId = Guid.NewGuid().ToString();
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Erreur de validation id {ErrorId}", errorId);
            await HandleExceptionAsync(context, vex, errorId, HttpStatusCode.BadRequest);
        }
        catch (UnauthorizedAccessException uex)
        {
            _logger.LogWarning(uex, "Accès refusé id {ErrorId}", errorId);
            await HandleExceptionAsync(context, uex, errorId, HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur interne id {ErrorId}", errorId);
            await HandleExceptionAsync(context, ex, errorId, HttpStatusCode.InternalServerError);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string errorId, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        var isDev = env?.EnvironmentName == "Development";

        var response = new ErrorResponse(
            context.Response.StatusCode,
            statusCode switch
            {
                HttpStatusCode.BadRequest => "Erreur de validation.",
                HttpStatusCode.Unauthorized => "Accès non autorisé.",
                _ => "Une erreur interne du serveur est survenue."
            },
            isDev ? $"{exception.Message} (id: {errorId})" : $"Erreur id: {errorId}"
        );

        return context.Response.WriteAsJsonAsync(response);
    }
}