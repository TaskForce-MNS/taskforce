using Api.Back.DTOs.Responses;
using Api.Back.Models;

namespace Api.Back.Extensions
{
    public static class ProjectExtensions
    {
        public static ProjectResponse ToResponse(this DbProject project)
        {
            ArgumentNullException.ThrowIfNull(project);
            return new ProjectResponse(
                project.Id,
                project.Name,
                project.Description,
                project.ColorHex,
                project.ImageUrl,
                project.CreatedById,
                project.CreatedAt,
                project.UpdatedAt
            );
        }
    }
}