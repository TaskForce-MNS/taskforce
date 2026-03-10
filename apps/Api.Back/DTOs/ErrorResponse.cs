
namespace Api.Back.DTOs;

public record ErrorResponse(int StatusCode, string Message, string? Details = null);