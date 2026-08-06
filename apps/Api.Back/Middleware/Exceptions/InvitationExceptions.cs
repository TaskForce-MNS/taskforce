namespace Api.Back.Middleware.Exceptions
{
    public class InvitationNotFoundException : Exception
    {
        public InvitationNotFoundException() : base("Invitation introuvable.") { }
        public InvitationNotFoundException(string message) : base(message) { }
        public InvitationNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvitationExpiredException : Exception
    {
        public InvitationExpiredException() : base("Cette invitation a expiré.") { }
        public InvitationExpiredException(string message) : base(message) { }
        public InvitationExpiredException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvitationExhaustedException : Exception
    {
        public InvitationExhaustedException() : base("Cette invitation a atteint son nombre maximal d'utilisations.") { }
        public InvitationExhaustedException(string message) : base(message) { }
        public InvitationExhaustedException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NotProjectAdminException : Exception
    {
        public NotProjectAdminException() : base("Seuls les propriétaires ou administrateurs peuvent effectuer cette action.") { }
        public NotProjectAdminException(string message) : base(message) { }
        public NotProjectAdminException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class AlreadyProjectMemberException : Exception
    {
        public AlreadyProjectMemberException() : base("Vous êtes déjà membre de ce projet.") { }
        public AlreadyProjectMemberException(string message) : base(message) { }
        public AlreadyProjectMemberException(string message, Exception innerException) : base(message, innerException) { }
    }
}