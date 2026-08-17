using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Back.Data;
using Api.Back.Models;
using Api.Back.DTOs.Requests.Projects;
using System.Security.Claims;
using Api.Back.Services;
using Api.Back.DTOs.Responses;
using Api.Back.Common;
using FluentValidation;

namespace Api.Back.Controllers.Projects
{

    [ApiController]
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly IProjectService _projectService;
        private readonly IValidator<PostProjectRequest> _validator;

        public ProjectsController(IProjectService projectService, IValidator<PostProjectRequest> validator)
        {
            _projectService = projectService;
            _validator = validator;
        }

        [HttpPost(BackUrls.PostProject)]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostProject([FromBody] PostProjectRequest request)
        {
            var userId = GetCurrentIdentityId();
            var validation = await _validator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);
            var response = await _projectService.PostProjectAsync(request, userId);

            return CreatedAtAction(nameof(GetProjectById), new { id = response.Id }, response);
        }

        [HttpGet(BackUrls.ListProjects)]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListUserProjects()
        {
            var userId = GetCurrentIdentityId();
            var projects = await _projectService.ListUserProjectsAsync(userId);

            return Ok(projects);
        }

        [HttpGet(BackUrls.GetProject)]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var userId = GetCurrentIdentityId();
            var project = await _projectService.GetProjectByIdAsync(id, userId);

            if (project == null)
                return NotFound("Projet introuvable ou accès non autorisé.");

            return Ok(project);
        }

        [HttpPut(BackUrls.PutProject)]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PutProject(Guid id, [FromBody] PutProjectRequest request)
        {
            var userId = GetCurrentIdentityId();
            var response = await _projectService.PutProjectAsync(id, request, userId);

            return Ok(response);
        }

        [HttpPatch(BackUrls.PatchProject)]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PatchProject(Guid id, [FromBody] PatchProjectRequest request)
        {
            var userId = GetCurrentIdentityId();
            var response = await _projectService.PatchProjectAsync(id, request, userId);

            return Ok(response);
        }

        [HttpGet(BackUrls.ListMembers)]
        public async Task<ActionResult<IEnumerable<ProjectMemberResponse>>> GetMembers(Guid projectId)
        {
            var members = await _projectService.GetProjectMembersAsync(projectId);
            return Ok(members);
        }
    }
}