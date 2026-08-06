using Api.Back.Data;
using Api.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Repositories
{
    public interface IInvitationRepository
    {
        Task AddAsync(DbInvitation invitation);
        Task<DbInvitation?> GetByIdAsync(Guid id);
        Task<DbInvitation?> GetByTokenAsync(string token);
        Task<IEnumerable<DbInvitation>> GetActiveByProjectAsync(Guid projectId);
        Task UpdateAsync(DbInvitation invitation);
        Task DeleteAsync(DbInvitation invitation);
    }

    public class InvitationRepository : IInvitationRepository
    {
        private readonly AppDbContext _context;

        public InvitationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DbInvitation invitation)
        {
            await _context.Invitations.AddAsync(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task<DbInvitation?> GetByIdAsync(Guid id) =>
            await _context.Invitations.FindAsync(id);

        public async Task<DbInvitation?> GetByTokenAsync(string token) =>
            await _context.Invitations.FirstOrDefaultAsync(i => i.Token == token);

        public async Task<IEnumerable<DbInvitation>> GetActiveByProjectAsync(Guid projectId) =>
            await _context.Invitations
                .Where(i => i.ProjectId == projectId
                         && i.ExpiresAt > DateTime.UtcNow
                         && (i.UsesLeft == null || i.UsesLeft > 0))
                .ToListAsync();

        public async Task UpdateAsync(DbInvitation invitation)
        {
            _context.Invitations.Update(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DbInvitation invitation)
        {
            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();
        }
    }
}