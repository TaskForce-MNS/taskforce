namespace Api.Back.DTOs.Requests
{
    public record UserRegisterDto(
        string Email,
        string Password,
        string FirstName,
        string Name,
        string Title,
        string Experience
    );

}