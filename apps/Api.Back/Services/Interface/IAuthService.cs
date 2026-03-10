using Api.Back.DTOs;
using Api.Back.DTOs.Requests;
using Api.Back.DTOs.Responses;

namespace Api.Back.Services.Interface;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(UserRegisterDto dto);

}