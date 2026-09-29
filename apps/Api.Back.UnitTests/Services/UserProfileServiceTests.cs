using Api.Back.DTOs.Requests.User;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Services
{
    public class UserProfileServiceTests
    {
        private readonly Mock<IIdentityRepository> _repositoryMock = new();
        private readonly UserProfileService _sut;

        public UserProfileServiceTests()
        {
            _sut = new UserProfileService(_repositoryMock.Object);
        }

        [Fact]
        public async Task AddSkillAsync_Should_CreateSkill_And_ReturnMappedDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new AddUserSkillRequest("  Figma  ", ExperienceLevel.Senior);

            _repositoryMock
                .Setup(r => r.AddSkillAsync(It.IsAny<DbUserSkill>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddSkillAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Domain.Should().Be("Figma");
            result.Level.Should().Be(ExperienceLevel.Senior);
            result.Id.Should().NotBeEmpty();

            _repositoryMock.Verify(r => r.AddSkillAsync(It.Is<DbUserSkill>(s =>
                s.UserId == userId &&
                s.Domain == "Figma" &&
                s.Level == ExperienceLevel.Senior
            )), Times.Once);
        }

        [Fact]
        public async Task RemoveSkillAsync_Should_ReturnTrue_And_RemoveSkill_When_SkillExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();
            var existingSkill = new DbUserSkill { Id = skillId, UserId = userId, Domain = "C#", Level = ExperienceLevel.Senior };

            _repositoryMock
                .Setup(r => r.GetSkillByIdAsync(skillId, userId))
                .ReturnsAsync(existingSkill);

            _repositoryMock
                .Setup(r => r.RemoveSkillAsync(existingSkill))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RemoveSkillAsync(userId, skillId);

            // Assert
            result.Should().BeTrue();

            _repositoryMock.Verify(r => r.RemoveSkillAsync(existingSkill), Times.Once);
        }

        [Fact]
        public async Task RemoveSkillAsync_Should_ReturnFalse_When_SkillNotFoundOrNotOwned()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var skillId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetSkillByIdAsync(skillId, userId))
                .ReturnsAsync((DbUserSkill?)null);

            // Act
            var result = await _sut.RemoveSkillAsync(userId, skillId);

            // Assert
            result.Should().BeFalse();

            _repositoryMock.Verify(r => r.RemoveSkillAsync(It.IsAny<DbUserSkill>()), Times.Never);
        }
    }
}