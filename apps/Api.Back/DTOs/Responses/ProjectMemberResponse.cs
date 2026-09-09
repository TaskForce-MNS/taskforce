namespace Api.Back.DTOs.Responses
{
    public record ProjectMemberResponse(
        Guid UserId,
        string Role,
        DateTime JoinedAt,
        string FirstName,
        string LastName
    );
}