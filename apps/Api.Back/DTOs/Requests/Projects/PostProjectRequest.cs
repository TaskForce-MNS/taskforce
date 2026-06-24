namespace Api.Back.DTOs.Requests.Projects
{
    public record PostProjectRequest(
        string Name,
        string? Description,
        string? ColorHex,
        string? ImageUrl
    );
}
