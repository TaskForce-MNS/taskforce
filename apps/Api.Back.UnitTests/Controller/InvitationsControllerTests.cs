using Api.Back.Controllers.Invitation;
using Api.Back.DTOs.Requests.invitation;
using Api.Back.DTOs.Responses.invitation;
using Api.Back.Middleware.Exceptions;
using Api.Back.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Api.Back.UnitTests.Controllers.Invitation
{
    public class InvitationsControllerTests
    {
        private readonly Mock<IInvitationService> _serviceMock = new();
        private readonly InvitationsController _sut;

        public InvitationsControllerTests()
        {
            _sut = new InvitationsController(_serviceMock.Object);
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

        private static InvitationResponse CreateInvitationResponse(Guid? id = null, Guid? projectId = null) => new(
            id ?? Guid.NewGuid(),
            projectId ?? Guid.NewGuid(),
            "fake-token-abc123",
            DateTime.UtcNow.AddDays(7),
            5,
            DateTime.UtcNow
        );

        // ------------------- CreateInvitation -------------------

        [Fact]
        public async Task CreateInvitation_Should_ReturnCreatedAtAction_With_Response()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new CreateInvitationRequest(7, 5);
            var response = CreateInvitationResponse(projectId: projectId);

            _serviceMock.Setup(s => s.CreateInvitationAsync(projectId, userId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.CreateInvitation(projectId, request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(InvitationsController.ListInvitations));
            createdResult.RouteValues!["projectId"].Should().Be(projectId);
            createdResult.Value.Should().Be(response);

            _serviceMock.Verify(s => s.CreateInvitationAsync(projectId, userId, request), Times.Once);
        }

        // ------------------- ListInvitations -------------------

        [Fact]
        public async Task ListInvitations_Should_ReturnOk_With_Invitations()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            SetUser(_sut, userId);

            var invitations = new List<InvitationResponse> { CreateInvitationResponse(), CreateInvitationResponse() };

            _serviceMock.Setup(s => s.ListActiveInvitationsAsync(projectId, userId))
                .ReturnsAsync(invitations);

            // Act
            var result = await _sut.ListInvitations(projectId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(invitations);
        }

        // ------------------- RevokeInvitation -------------------

        [Fact]
        public async Task RevokeInvitation_Should_ReturnNoContent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();
            SetUser(_sut, userId);

            _serviceMock.Setup(s => s.RevokeInvitationAsync(invitationId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RevokeInvitation(invitationId);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            _serviceMock.Verify(s => s.RevokeInvitationAsync(invitationId, userId), Times.Once);
        }

        // ------------------- AcceptInvitation -------------------

        [Fact]
        public async Task AcceptInvitation_Should_ReturnOk_With_ProjectId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new AcceptInvitationRequest("valid-token");

            _serviceMock.Setup(s => s.AcceptInvitationAsync(request.Token, userId))
                .ReturnsAsync(projectId);

            // Act
            var result = await _sut.AcceptInvitation(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { Message = "Vous avez rejoint le projet !", ProjectId = projectId });
        }

        // ------------------- JoinOrganization -------------------

        [Fact]
        public async Task JoinOrganization_Should_ReturnBadRequest_When_TokenIsMissing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var dto = new JoinOrganizationRequestDto("   ");

            // Act
            var result = await _sut.JoinOrganization(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            _serviceMock.Verify(s => s.AcceptInvitationAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task JoinOrganization_Should_ReturnOk_When_TokenIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            SetUser(_sut, userId);

            var dto = new JoinOrganizationRequestDto("valid-token");

            _serviceMock.Setup(s => s.AcceptInvitationAsync(dto.Token, userId))
                .ReturnsAsync(projectId);

            // Act
            var result = await _sut.JoinOrganization(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { Message = "Vous avez rejoint le projet avec succès !", ProjectId = projectId });
        }

        [Theory]
        [InlineData(typeof(InvitationNotFoundException), "Le lien ou code d'invitation est invalide.")]
        [InlineData(typeof(InvitationExpiredException), "Cette invitation a expiré.")]
        [InlineData(typeof(InvitationExhaustedException), "Cette invitation a déjà été utilisée son nombre maximum de fois.")]
        [InlineData(typeof(AlreadyProjectMemberException), "Vous êtes déjà membre de ce projet.")]
        public async Task JoinOrganization_Should_ReturnBadRequest_When_ServiceThrowsKnownException(Type exceptionType, string expectedMessage)
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var dto = new JoinOrganizationRequestDto("some-token");
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            _serviceMock.Setup(s => s.AcceptInvitationAsync(dto.Token, userId))
                .ThrowsAsync(exception);

            // Act
            var result = await _sut.JoinOrganization(dto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = expectedMessage });
        }

        [Fact]
        public async Task JoinOrganization_Should_Rethrow_When_ServiceThrowsUnknownException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var dto = new JoinOrganizationRequestDto("some-token");

            _serviceMock.Setup(s => s.AcceptInvitationAsync(dto.Token, userId))
                .ThrowsAsync(new InvalidOperationException("Erreur inattendue"));

            // Act
            var act = async () => await _sut.JoinOrganization(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Erreur inattendue");
        }
    }
}