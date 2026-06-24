namespace Api.Back.DTOs.Requests.Projects
{
    public record PatchProjectRequest(
        string? Name = null,
        string? Description = null,
        string? ColorHex = null,
        string? ImageUrl = null
    );
}
