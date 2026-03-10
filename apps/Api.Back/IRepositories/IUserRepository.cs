using Api.Back.Models;

namespace Api.Back.IRepositories
{
    public interface IUserRepository
    {
        Task<DbUser?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(DbUser user);
    }
}
