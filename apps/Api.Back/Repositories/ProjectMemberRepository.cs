using Api.Back.Data;
using Api.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Repositories
{
    public interface IProjectMemberRepository
    {
        Task AddAsync(DbProjectMember member);
        Task<DbProjectMember?> GetAsync(Guid projectId, Guid identityId);
        Task<IEnumerable<DbProjectMember>> GetByProjectIdAsync(Guid projectId);
        Task<bool> IsMemberAsync(Guid projectId, Guid identityId);
    }

    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly AppDbContext _context;

        public ProjectMemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DbProjectMember member)
        {
            await _context.ProjectMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task<DbProjectMember?> GetAsync(Guid projectId, Guid identityId)
        {
            return await _context.ProjectMembers.FindAsync(projectId, identityId);
        }

        public async Task<IEnumerable<DbProjectMember>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.Identity)
                .ToListAsync();
        }
        public async Task<bool> IsMemberAsync(Guid projectId, Guid identityId)
        {
            return await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.IdentityId == identityId);
        }
    }
}