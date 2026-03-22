using Api.Back.DTOs.Requests;
using Api.Back.DTOs.Responses;
using Api.Back.IRepositories;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Tools;
using Api.Back.Validators;
using BCrypt.Net;

namespace Api.Back.Services;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(UserRegisterDto dto);

}
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordValidator _passwordValidator;

    public AuthService(IUserRepository userRepository, IPasswordValidator passwordValidator)
    {
        _userRepository = userRepository;
        _passwordValidator = passwordValidator;
    }

    public async Task<UserResponseDto> RegisterAsync(UserRegisterDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (!_passwordValidator.PasswordIsValid(dto.Password))
        {
            throw new WeakPasswordException();
        }

        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            throw new EmailAlreadyExistsException();
        }

        var defaultPreference = new Preference
        {
            Id = Guid.NewGuid(),
            Theme = "light",
            Appearance = "modern",
            FontSize = 14,
            IsAutoTheme = true
        };

        var user = new DbUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            Name = dto.Name,
            Title = dto.Title,
            Experience = dto.Experience,
            CreatedAt = DateTime.UtcNow,
            CurrentWorkload = 0,
            PreferenceId = defaultPreference.Id,
            Preference = defaultPreference
        };

        await _userRepository.AddAsync(user);

        return new UserResponseDto(
            Id: user.Id,
            Email: user.Email,
            FullName: $"{user.FirstName} {user.Name}",
            Title: user.Title,
            Workload: user.CurrentWorkload,
            Experience: user.Experience,
            CreatedAt: user.CreatedAt
        );
    }
}