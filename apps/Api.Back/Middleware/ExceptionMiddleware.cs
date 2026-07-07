using System.Net;
using Api.Back.DTOs;
using FluentValidation;
using Api.Back.Middleware.Exceptions;
namespace Api.Back.Middleware
{
    public partial class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Erreur de validation id {ErrorId}")]
        private partial void LogValidationError(ValidationException ex, string errorId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Accès refusé id {ErrorId}")]
        private partial void LogUnauthorizedAccessError(UnauthorizedAccessException ex, string errorId);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Erreur interne id {ErrorId}")]
        private partial void LogInternalError(Exception ex, string errorId);

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var errorId = Guid.NewGuid().ToString();

            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                LogValidationError(vex, errorId);
                await HandleExceptionAsync(context, vex, errorId, HttpStatusCode.BadRequest);
            }
            catch (UnauthorizedAccessException uex)
            {
                LogUnauthorizedAccessError(uex, errorId);
                await HandleExceptionAsync(context, uex, errorId, HttpStatusCode.Unauthorized);
            }
            catch (ProjectNotFoundException pnfEx)
            {
                await HandleExceptionAsync(context, pnfEx, errorId, HttpStatusCode.NotFound);
            }
            catch (ProjectForbiddenException pfEx)
            {
                await HandleExceptionAsync(context, pfEx, errorId, HttpStatusCode.Forbidden);
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                LogInternalError(ex, errorId);
                await HandleExceptionAsync(context, ex, errorId, HttpStatusCode.InternalServerError);
            }
#pragma warning restore CA1031
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
                    HttpStatusCode.Forbidden => "Accès interdit.",        
                    HttpStatusCode.NotFound => "Ressource introuvable.", 
                    _ => "Une erreur interne du serveur est survenue."
                },
                isDev ? $"{exception.Message} (id: {errorId})" : $"Erreur id: {errorId}"
            );

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}