using System.Security.Claims;
using Api.Back.Controllers;
using Api.Back.DTOs.Requests.User;
using Api.Back.DTOs.Responses;
using Api.Back.Models;
using Api.Back.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Controller
{
    public class UserControllerTests
    {
        private readonly Mock<IUserProfileService> _serviceMock = new();
        private readonly UserController _sut;

        public UserControllerTests()
        {
            _sut = new UserController(_serviceMock.Object);
        }
        private static void SetUser(ControllerBase controller, Guid? userId)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
        [Fact]
        public async Task AddSkill_Should_ReturnOk_With_CreatedSkill()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new AddUserSkillRequest("React", ExperienceLevel.Intermediate);
            var response = new UserSkillDto(Guid.NewGuid(), "React", ExperienceLevel.Intermediate);

            _serviceMock.Setup(s => s.AddSkillAsync(userId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.AddSkill(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);

            _serviceMock.Verify(s => s.AddSkillAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task RemoveSkill_Should_ReturnNoContent_When_SkillDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            SetUser(_sut, userId);

            _serviceMock.Setup(s => s.RemoveSkillAsync(userId, skillId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.RemoveSkill(skillId);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            _serviceMock.Verify(s => s.RemoveSkillAsync(userId, skillId), Times.Once);
        }

        [Fact]
        public async Task RemoveSkill_Should_ReturnNotFound_When_SkillDoesNotExistOrNotAuthorized()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            SetUser(_sut, userId);

            _serviceMock.Setup(s => s.RemoveSkillAsync(userId, skillId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.RemoveSkill(skillId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();

            _serviceMock.Verify(s => s.RemoveSkillAsync(userId, skillId), Times.Once);
        }
    }
}