using Api.Back.DTOs.Requests.Projects;
using Api.Back.DTOs.Responses;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Extensions;
using Api.Back.Middleware.Exceptions;

namespace Api.Back.Services
{
    public interface IProjectService
    {
        Task<ProjectResponse> PostProjectAsync(PostProjectRequest request, Guid userId);
        Task<ProjectResponse?> GetProjectByIdAsync(Guid id, Guid userId);
        Task<IEnumerable<ProjectResponse>> ListUserProjectsAsync(Guid userId);
        Task<ProjectResponse> PutProjectAsync(Guid id, PutProjectRequest request, Guid userId);
        Task<ProjectResponse> PatchProjectAsync(Guid id, PatchProjectRequest request, Guid userId);
    }
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;

        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProjectResponse> PostProjectAsync(PostProjectRequest request, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var newProject = new DbProject
            {
                Name = request.Name,
                Description = request.Description,
                ColorHex = request.ColorHex,
                ImageUrl = request.ImageUrl,
                CreatedById = userId
            };

            await _repository.AddAsync(newProject);
            return newProject.ToResponse();
        }
        public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id, Guid userId)
        {
            var project = await _repository.GetByIdAsync(id);

            if (project == null || project.CreatedById != userId)
            {
                return null;
            }

            return project.ToResponse();
        }
        public async Task<IEnumerable<ProjectResponse>> ListUserProjectsAsync(Guid userId)
        {
            var projects = await _repository.GetByUserAsync(userId);

            return projects.Select(p => p.ToResponse());
        }

        public async Task<ProjectResponse> PutProjectAsync(Guid id, PutProjectRequest request, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var project = await GetAndAuthorizeAsync(id, userId);

            project.Name = request.Name;
            project.Description = request.Description;
            project.ColorHex = request.ColorHex;
            project.ImageUrl = request.ImageUrl;
            project.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(project);
            return project.ToResponse();
        }

        public async Task<ProjectResponse> PatchProjectAsync(Guid id, PatchProjectRequest request, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var project = await GetAndAuthorizeAsync(id, userId);

            project.Name = request.Name ?? project.Name;
            project.Description = request.Description ?? project.Description;
            project.ColorHex = request.ColorHex ?? project.ColorHex;
            project.ImageUrl = request.ImageUrl ?? project.ImageUrl;
            project.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(project);
            return project.ToResponse();
        }
        private async Task<DbProject> GetAndAuthorizeAsync(Guid id, Guid userId)
        {
            var project = await _repository.GetByIdAsync(id);

            if (project == null)
                throw new ProjectNotFoundException();

            if (project.CreatedById != userId)
                throw new ProjectForbiddenException();

            return project;
        }
    }
}
