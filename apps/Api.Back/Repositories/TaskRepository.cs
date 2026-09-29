using Api.Back.Data;
using Api.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Repositories
{
    public interface ITaskRepository
    {
        Task<List<DbTask>> GetByProjectIdAsync(Guid projectId);
        Task<DbTask?> GetByIdAsync(Guid taskId);
        Task AddAsync(DbTask task);
        Task UpdateAsync(DbTask task);
    }

    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DbTask>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .Where(t => t.ProjectId == projectId && !t.IsArchived)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<DbTask?> GetByIdAsync(Guid taskId)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task AddAsync(DbTask task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DbTask task)
        {
            await _context.SaveChangesAsync();
            await _context.Entry(task).Reference(t => t.Assignee).LoadAsync();
        }
    }
}