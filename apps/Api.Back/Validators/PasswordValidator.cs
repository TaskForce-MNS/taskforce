namespace Api.Back.Validators
{
    public interface IPasswordValidator
    {
        bool PasswordIsValid(string password);
    }
    public class PasswordValidator : IPasswordValidator
    {
        private static readonly System.Buffers.SearchValues<char> s_specialChars = System.Buffers.SearchValues.Create("!@#$%^&*");

        public bool PasswordIsValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (password.AsSpan().IndexOfAny(s_specialChars) == -1) return false;

            return true;
        }
    }

}