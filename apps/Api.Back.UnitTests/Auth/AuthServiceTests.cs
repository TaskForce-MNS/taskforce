using Api.Back.DTOs.Requests;
using Api.Back.Models;
using Api.Back.IRepositories;
using Api.Back.Services;
using FluentAssertions;
using Moq;
using Api.Back.Tools;
using Api.Back.Validators;
using Xunit;

namespace Api.Back.UnitTests.Auth
{
    public class AuthServiceTests
    {
        // On déclare les Mocks (les doublures)
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly AuthService _authService;
        private readonly Mock<IPasswordValidator> _passwordValidatorMock;

        public AuthServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _passwordValidatorMock = new Mock<IPasswordValidator>();
            _authService = new AuthService(_mockRepo.Object, _passwordValidatorMock.Object);

            _passwordValidatorMock
                .Setup(v => v.PasswordIsValid(It.IsAny<string>()))
                .Returns(true);
        }

        [Fact]
        public async Task RegisterAsync_Should_ReturnUser_When_DataIsValid()
        {
            var dto = new UserRegisterDto("test@test.com", "Password123!", "Jean", "Test", "Dev", "Junior");

            _mockRepo.Setup(repo => repo.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);

            var result = await _authService.RegisterAsync(dto);

            result.Should().NotBeNull();
            result.Email.Should().Be(dto.Email);
            result.Workload.Should().Be(0);
            result.FullName.Should().Be("Jean Test");
            result.Title.Should().Be(dto.Title);
            result.Experience.Should().Be(dto.Experience);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<DbUser>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Should_ThrowException_When_EmailAlreadyExists()
        {
            // ARRANGE
            var dto = new UserRegisterDto("existe@deja.com", "Password123!", "Jean", "Test", "Dev", "10");

            _mockRepo.Setup(repo => repo.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(true);

            // ACT & ASSERT 
            await Assert.ThrowsAsync<EmailAlreadyExistsException>(() => _authService.RegisterAsync(dto));

            _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<DbUser>()), Times.Never);
        }
        [Fact]
        public async Task RegisterAsync_Should_ThrowException_When_PasswordIsWeak()
        {
            // ARRANGE
            var dto = new UserRegisterDto("test@test.com", "weakpwd", "Jean", "Test", "Dev", "Junior");

            _mockRepo.Setup(repo => repo.EmailExistsAsync(dto.Email)).ReturnsAsync(false);

            _passwordValidatorMock.Setup(v => v.PasswordIsValid(dto.Password)).Returns(false);

            // ACT & ASSERT
            await Assert.ThrowsAsync<WeakPasswordException>(() => _authService.RegisterAsync(dto));

            _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<DbUser>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldHashPassword()
        {
            // Arrange
            var dto = new UserRegisterDto(
                "test@test.com",
                "Password123!",
                "Jean",
                "Test",
                "Dev",
                "Junior"
            );

            _mockRepo.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);

            _passwordValidatorMock.Setup(v => v.PasswordIsValid(dto.Password)).Returns(true);

            DbUser? savedUser = null;

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<DbUser>()))
                .Callback<DbUser>(u => savedUser = u)
                .Returns(Task.CompletedTask);

            // Act
            await _authService.RegisterAsync(dto);

            // Assert
            savedUser.Should().NotBeNull();
            savedUser!.PasswordHash.Should().NotBe(dto.Password);
            BCrypt.Net.BCrypt.Verify(dto.Password, savedUser.PasswordHash)
                .Should().BeTrue();
        }

    }

}
