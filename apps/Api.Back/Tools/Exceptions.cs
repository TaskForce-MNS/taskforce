namespace Api.Back.Tools
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException()
            : base("Cet email est déjà utilisé.") { }

        public EmailAlreadyExistsException(string message) : base(message)
        {
        }

        public EmailAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
    public class WeakPasswordException : Exception
    {
        public WeakPasswordException()
            : base("Le mot de passe ne respecte pas les règles de sécurité.") { }

        public WeakPasswordException(string message) : base(message)
        {
        }

        public WeakPasswordException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }

}