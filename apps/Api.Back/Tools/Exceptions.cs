namespace Api.Back.Tools
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException()
            : base("Cet email est déjà utilisé.") { }
    }
    public class WeakPasswordException : Exception
    {
        public WeakPasswordException()
            : base("Le mot de passe ne respecte pas les règles de sécurité.") { }
    }

}