using Api.Back.DTOs.Requests;
using Api.Back.Services;
using Api.Back.Common;
using Api.Back.Tools;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Back.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<UserRegisterDto> _validator;

        public AuthController(IAuthService authService, IValidator<UserRegisterDto> validator)
        {
            _authService = authService;
            _validator = validator;
        }

        [HttpPost(BackUrls.Register)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Register a new user", Description = "Registers")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            // 1. D'abord, on appelle la SÉCURITÉ (Guard)
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                // Si le garde dit non, on renvoie les erreurs tout de suite (400 Bad Request)
                return BadRequest(validationResult.Errors);
            }
            try
            {
                // 2. Service
                var userCreated = await _authService.RegisterUserAsync(dto);
                // 3. Succès
                return CreatedAtAction(nameof(Register), new { id = userCreated.Id }, userCreated);
            }
            catch (EmailAlreadyExistsException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
            catch (WeakPasswordException ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
