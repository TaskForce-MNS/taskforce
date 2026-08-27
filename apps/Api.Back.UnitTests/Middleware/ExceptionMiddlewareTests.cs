using System.Net;
using System.Text.Json;
using Api.Back.Middleware;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Api.Back.Middleware.Exceptions;
using Microsoft.Extensions.Logging;

namespace Api.Back.UnitTests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private static DefaultHttpContext CreateHttpContext(string environmentName)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns(environmentName);

            var services = new ServiceCollection();
            services.AddSingleton(envMock.Object);
            context.RequestServices = services.BuildServiceProvider();

            return context;
        }

        private static async Task<JsonDocument> ReadResponseBodyAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();
            return JsonDocument.Parse(body);
        }
        private static ILogger<ExceptionMiddleware> CreateEnabledLogger()
        {
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            return loggerMock.Object;
        }
        [Fact]
        public async Task InvokeAsync_Should_CallNext_When_NoExceptionIsThrown()
        {
            // Arrange
            var context = CreateHttpContext("Production");
            var nextCalled = false;
            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ExceptionMiddleware(next, CreateEnabledLogger());

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200); // valeur par défaut, non modifiée
        }

        [Theory]
        [MemberData(nameof(ExceptionScenarios))]
        public async Task InvokeAsync_Should_ReturnExpectedStatusCode_And_Message(
            Exception exceptionToThrow, HttpStatusCode expectedStatusCode, string expectedMessage)
        {
            // Arrange
            var context = CreateHttpContext("Production");
            Task next(HttpContext _) => throw exceptionToThrow;
            var middleware = new ExceptionMiddleware(next, NullLogger<ExceptionMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)expectedStatusCode);
            context.Response.ContentType.Should().StartWith("application/json");

            using var json = await ReadResponseBodyAsync(context);
            var root = json.RootElement;

            var messageFound = root.EnumerateObject()
                .Any(p => p.Value.ValueKind == JsonValueKind.String && p.Value.GetString() == expectedMessage);

            messageFound.Should().BeTrue($"la réponse devrait contenir le message '{expectedMessage}'");
        }

        public static IEnumerable<object[]> ExceptionScenarios()
        {
            yield return new object[] { new ValidationException("invalid"), HttpStatusCode.BadRequest, "Erreur de validation." };
            yield return new object[] { new UnauthorizedAccessException(), HttpStatusCode.Unauthorized, "Accès non autorisé." };
            yield return new object[] { new ProjectForbiddenException(), HttpStatusCode.Forbidden, "Accès interdit." };
            yield return new object[] { new ProjectNotFoundException(), HttpStatusCode.NotFound, "Ressource introuvable." };
            yield return new object[] { new InvalidOperationException("boom"), HttpStatusCode.InternalServerError, "Une erreur interne du serveur est survenue." };

            yield return new object[] { new InvitationNotFoundException("Invitation introuvable."), HttpStatusCode.NotFound, "Invitation introuvable." };
            yield return new object[] { new InvitationExpiredException("Le lien a expiré."), HttpStatusCode.Gone, "Le lien a expiré." };
            yield return new object[] { new InvitationExhaustedException("Limite d'utilisation atteinte."), HttpStatusCode.Conflict, "Limite d'utilisation atteinte." };
            yield return new object[] { new AlreadyProjectMemberException("L'utilisateur est déjà membre."), HttpStatusCode.Conflict, "L'utilisateur est déjà membre." };
            yield return new object[] { new NotProjectAdminException("Seul un admin peut faire cela."), HttpStatusCode.Forbidden, "Seul un admin peut faire cela." };
        }

        [Fact]
        public async Task InvokeAsync_Should_IncludeRealExceptionMessage_When_EnvironmentIsDevelopment()
        {
            // Arrange
            var context = CreateHttpContext("Development");
            RequestDelegate next = _ => throw new InvalidOperationException("Erreur précise pour debug");
            var middleware = new ExceptionMiddleware(next, NullLogger<ExceptionMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            using var json = await ReadResponseBodyAsync(context);
            var body = json.RootElement.ToString();

            body.Should().Contain("Erreur précise pour debug");
        }

        [Fact]
        public async Task InvokeAsync_Should_HideRealExceptionMessage_When_EnvironmentIsProduction()
        {
            // Arrange
            var context = CreateHttpContext("Production");
            RequestDelegate next = _ => throw new InvalidOperationException("Erreur précise pour debug");
            var middleware = new ExceptionMiddleware(next, NullLogger<ExceptionMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            using var json = await ReadResponseBodyAsync(context);
            var body = json.RootElement.ToString();

            body.Should().NotContain("Erreur précise pour debug");
            body.Should().Contain("Erreur id:");
        }

        [Fact]
        public async Task InvokeAsync_Should_ThrowArgumentNullException_When_ContextIsNull()
        {
            // Arrange
            RequestDelegate next = _ => Task.CompletedTask;
            var middleware = new ExceptionMiddleware(next, NullLogger<ExceptionMiddleware>.Instance);

            // Act
            var act = async () => await middleware.InvokeAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task InvokeAsync_Should_HandleNullEnvironmentGracefully()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            // On crée des services VIDES (sans IWebHostEnvironment)
            context.RequestServices = new ServiceCollection().BuildServiceProvider();

            RequestDelegate next = static _ => throw new InvalidOperationException("Erreur fatale");
            var middleware = new ExceptionMiddleware(next, CreateEnabledLogger());

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(500);
        }
    }
}