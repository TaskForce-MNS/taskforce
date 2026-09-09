namespace Api.Back.DTOs.Requests.Task
{
    public record PostTaskRequest(
            string Name,
            string? Description,
            Guid ProjectId
        );
}