// Middleware/Exceptions/ProjectExceptions.cs
public class ProjectNotFoundException : Exception { public ProjectNotFoundException() : base("Projet introuvable.") { } }
public class ProjectForbiddenException : Exception { public ProjectForbiddenException() : base("Accès interdit à ce projet.") { } }