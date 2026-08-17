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
        Task<IEnumerable<ProjectMemberResponse>> GetProjectMembersAsync(Guid projectId);
    }
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly IProjectMemberRepository _memberRepository;

        public ProjectService(IProjectRepository repository, IProjectMemberRepository memberRepository)
        {
            _repository = repository;
            _memberRepository = memberRepository;
        }

        public async Task<ProjectResponse> PostProjectAsync(PostProjectRequest request, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var projectId = Guid.NewGuid();
            var newProject = new DbProject
            {
                Id = projectId,
                Name = request.Name,
                Description = request.Description,
                ColorHex = request.ColorHex,
                ImageUrl = request.ImageUrl,
                CreatedById = userId
            };

            newProject.Members.Add(new DbProjectMember
            {
                ProjectId = newProject.Id,
                IdentityId = userId,
                Role = ProjectMemberRole.Owner
            });

            await _repository.AddAsync(newProject);
            return newProject.ToResponse(ProjectMemberRole.Owner);
        }
        public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id, Guid userId)
        {
            var project = await _repository.GetByIdAsync(id);
            if (project == null) return null;

            var membership = await _memberRepository.GetAsync(id, userId);
            if (membership == null) return null;

            return project.ToResponse(membership.Role);
        }
        public async Task<IEnumerable<ProjectResponse>> ListUserProjectsAsync(Guid userId)
        {
            var projects = await _repository.GetByUserAsync(userId);

            return projects.Select(p =>
            {
                var member = p.Members.FirstOrDefault(m => m.IdentityId == userId);

                var userRole = member != null ? member.Role : ProjectMemberRole.Member;

                return p.ToResponse(userRole);
            });
        }

        public async Task<ProjectResponse> PutProjectAsync(Guid id, PutProjectRequest request, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var project = await GetAndAuthorizeAdminAsync(id, userId);

            project.Name = request.Name;
            project.Description = request.Description;
            project.ColorHex = request.ColorHex;
            project.ImageUrl = request.ImageUrl;
            project.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(project);
            var membership = await _memberRepository.GetAsync(id, userId);
            return project.ToResponse(membership!.Role);
        }

        public async Task<ProjectResponse> PatchProjectAsync(Guid id, PatchProjectRequest request, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var project = await GetAndAuthorizeAdminAsync(id, userId);

            project.Name = request.Name ?? project.Name;
            project.Description = request.Description ?? project.Description;
            project.ColorHex = request.ColorHex ?? project.ColorHex;
            project.ImageUrl = request.ImageUrl ?? project.ImageUrl;
            project.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(project);
            var membership = await _memberRepository.GetAsync(id, userId);
            return project.ToResponse(membership!.Role);
        }

        public async Task<IEnumerable<ProjectMemberResponse>> GetProjectMembersAsync(Guid projectId)
        {
            var members = await _memberRepository.GetByProjectIdAsync(projectId);

            return members.Select(m => new ProjectMemberResponse(
                m.IdentityId,
                m.Role.ToString(),
                m.JoinedAt,
                m.Identity.FirstName,
                m.Identity.LastName
            ));
        }
        private async Task<DbProject> GetAndAuthorizeAdminAsync(Guid id, Guid userId)
        {
            var project = await _repository.GetByIdAsync(id)
                ?? throw new ProjectNotFoundException();

            var membership = await _memberRepository.GetAsync(id, userId);
            if (membership == null || membership.Role == ProjectMemberRole.Member)
                throw new ProjectForbiddenException();

            return project;
        }
    }
}