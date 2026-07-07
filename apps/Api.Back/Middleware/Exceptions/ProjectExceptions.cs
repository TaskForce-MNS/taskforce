using System;

namespace Api.Back.Middleware.Exceptions;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException() : base("Projet introuvable.") { }

    public ProjectNotFoundException(string message) : base(message) { }

    public ProjectNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

public class ProjectForbiddenException : Exception
{
    public ProjectForbiddenException() : base("Accès interdit à ce projet.") { }

    public ProjectForbiddenException(string message) : base(message) { }

    public ProjectForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}