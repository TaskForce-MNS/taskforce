using Api.Back.Validators;
using FluentAssertions;
using Xunit;

namespace Api.Back.UnitTests.Validators
{
    public class PasswordValidatorTests
    {
        private readonly PasswordValidator _validator = new();

        [Theory]
        [InlineData("123456")]
        [InlineData("password")]
        [InlineData("no_upper_case_1")]
        [InlineData("NO_LOWER_CASE_1")]
        [InlineData("NoSpecialChar1")]
        [InlineData("")]
        public void IsValid_ShouldReturnFalse_ForWeakPasswords(string password)
        {
            var result = _validator.PasswordIsValid(password);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("Password123!")]
        [InlineData("Strong@Pass1")]
        [InlineData("Valid#2026")]
        public void IsValid_ShouldReturnTrue_ForStrongPasswords(string password)
        {
            var result = _validator.PasswordIsValid(password);
            result.Should().BeTrue();
        }
    }

}
