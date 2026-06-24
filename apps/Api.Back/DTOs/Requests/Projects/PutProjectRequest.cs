namespace Api.Back.DTOs.Requests.Projects
{
    public record PutProjectRequest
    (
        string Name,
        string? Description,
        string? ColorHex,
        string? ImageUrl
    );
}
