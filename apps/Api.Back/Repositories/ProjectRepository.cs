using Api.Back.Data;
using Api.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Repositories
{
    public interface IProjectRepository
    {
        Task AddAsync(DbProject project);
        Task<DbProject?> GetByIdAsync(Guid id);
        Task<IEnumerable<DbProject>> GetByUserAsync(Guid userId);
        Task UpdateAsync(DbProject project);
    }

    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DbProject project)
        {
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task<DbProject?> GetByIdAsync(Guid id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<IEnumerable<DbProject>> GetByUserAsync(Guid userId)
        {
            return await _context.Projects.Where(p => p.CreatedById == userId).ToListAsync();
        }
        public async Task UpdateAsync(DbProject project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }
    }

}
