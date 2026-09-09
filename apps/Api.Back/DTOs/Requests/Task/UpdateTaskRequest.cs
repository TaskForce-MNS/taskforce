namespace Api.Back.DTOs.Requests.Task
{
    public record UpdateTaskRequest(
        string? Name,
        string? Description,
        bool? IsChecked,
        bool? IsArchived
    );
}