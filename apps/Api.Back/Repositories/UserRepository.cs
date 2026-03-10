using Api.Back.Data;
using Api.Back.IRepositories;
using Api.Back.Models;
using Microsoft.EntityFrameworkCore;


namespace Api.Back.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DbUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(DbUser user)
    {
        // On ajoute l'utilisateur (EF Core gère aussi l'ajout de la Préférence liée)
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}