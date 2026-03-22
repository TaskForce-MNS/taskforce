namespace Api.Back.Validators
{
    public interface IPasswordValidator
    {
        bool PasswordIsValid(string password);
    }
    public class PasswordValidator : IPasswordValidator
    {
        public bool PasswordIsValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => "!@#$%^&*".Contains(ch, StringComparison.Ordinal))) return false;

            return true;
        }
    }

}