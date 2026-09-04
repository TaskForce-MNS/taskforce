using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Back.DTOs.Requests.Task;
using Api.Back.Services;
using FluentValidation;
using Api.Back.DTOs.Responses;
using Api.Back.Common;

namespace Api.Back.Controllers.Task
{
    [ApiController]
    [Authorize]
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly IValidator<PostTaskRequest> _postValidator;
        private readonly IValidator<UpdateTaskRequest> _updateValidator;

        public TaskController(
            ITaskService taskService,
            IValidator<PostTaskRequest> postValidator,
            IValidator<UpdateTaskRequest> updateValidator)
        {
            _taskService = taskService;
            _postValidator = postValidator;
            _updateValidator = updateValidator;
        }

        [HttpPost(BackUrls.PostTask)]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostTask([FromBody] PostTaskRequest request)
        {
            var validationResult = await _postValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = GetCurrentIdentityId();
            var response = await _taskService.PostAsync(request, userId);

            return CreatedAtAction(nameof(GetTaskById), new { taskId = response.Id }, response);
        }
        [HttpGet(BackUrls.GetTask)]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var userId = GetCurrentIdentityId();
            var task = await _taskService.GetTaskByIdAsync(taskId, userId);

            if (task == null)
                return NotFound("Tâche introuvable ou accès non autorisé.");

            return Ok(task);
        }

        [HttpGet(BackUrls.ListTasks)]
        [ProducesResponseType(typeof(IEnumerable<TaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListTasksByProject(Guid projectId)
        {
            var userId = GetCurrentIdentityId();
            var tasks = await _taskService.GetByProjectAsync(projectId, userId);
            return Ok(tasks);
        }

        [HttpPatch(BackUrls.UpdateTask)]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var userId = GetCurrentIdentityId();
            var response = await _taskService.UpdateAsync(taskId, request, userId);

            if (response == null)
            {
                return NotFound(new { message = "Tâche introuvable." });
            }

            return Ok(response);
        }
    }
}