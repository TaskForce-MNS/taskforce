
namespace Api.Back.DTOs;

// Un Record simple pour uniformiser les erreurs
public record ErrorResponse(int StatusCode, string Message, string? Details = null);