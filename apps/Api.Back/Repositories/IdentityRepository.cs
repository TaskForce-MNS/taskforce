using Api.Back.Data;
using Api.Back.DTOs.Responses;
using Api.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Repositories;

public interface IIdentityRepository
{
    Task<DbIdentity?> GetByPublicKeyAsync(string publicKey);
    Task<bool> PublicKeyExistsAsync(string publicKey);
    Task AddAsync(DbIdentity identity);
    Task<DbIdentity?> GetByCredentialIdAsync(byte[] descriptorId);
    Task UpdateSignatureCounterAsync(byte[] credentialId, uint newCounter);
    Task<UserResponseDto?> GetUserProfileByIdAsync(Guid identityId, CancellationToken cancellationToken);
    Task AddSkillAsync(DbUserSkill skill);
    Task<DbUserSkill?> GetSkillByIdAsync(Guid skillId, Guid userId);
    Task RemoveSkillAsync(DbUserSkill skill);
}

public class IdentityRepository : IIdentityRepository
{
    private readonly AppDbContext _context;

    public IdentityRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }
    public async Task<DbIdentity?> GetByPublicKeyAsync(string publicKey)
    {
        var publicKeyBytes = Convert.FromBase64String(publicKey);

        var credential = await _context.Credentials
            .Include(c => c.Identity)
            .ThenInclude(i => i!.Credentials)
            .Include(c => c.Identity)
            .ThenInclude(i => i!.Preference)
            .FirstOrDefaultAsync(c =>
                c.PublicKey.SequenceEqual(publicKeyBytes));

        return credential?.Identity;
    }

    public async Task<bool> PublicKeyExistsAsync(string publicKey)
    {
        var publicKeyBytes = Convert.FromBase64String(publicKey);
        return await _context.Credentials.AnyAsync(c => c.PublicKey.SequenceEqual(publicKeyBytes));
    }

    public async Task AddAsync(DbIdentity identity)
    {
        await _context.Identities.AddAsync(identity);
        await _context.SaveChangesAsync();
    }

    public async Task<DbIdentity?> GetByCredentialIdAsync(byte[] descriptorId)
    {
        return await _context.Identities
            .Include(i => i.Credentials)
            .FirstOrDefaultAsync(i => i.Credentials.Any(c => c.DescriptorId.SequenceEqual(descriptorId)));
    }

    public async Task UpdateSignatureCounterAsync(byte[] credentialId, uint newCounter)
    {
        var credential = await _context.Credentials
            .FirstOrDefaultAsync(c => c.DescriptorId == credentialId);

        if (credential != null)
        {
            credential.SignatureCounter = newCounter;
            await _context.SaveChangesAsync();
        }
    }
    public async Task<UserResponseDto?> GetUserProfileByIdAsync(Guid identityId, CancellationToken cancellationToken)
    {
        return await _context.Identities
            .AsNoTracking()
            .Where(i => i.Id == identityId && !i.IsDeleted)
            .Select(i => new UserResponseDto(
                i.Id,
                i.FirstName,
                i.LastName,
                i.Title,
                i.CurrentWorkload,
                i.Experience,
                i.CreatedAt,
                i.Skills.Select(s => new UserSkillDto(s.Id, s.Domain, s.Level)).ToArray()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task AddSkillAsync(DbUserSkill skill)
    {
        await _context.UserSkills.AddAsync(skill);
        await _context.SaveChangesAsync();
    }

    public async Task<DbUserSkill?> GetSkillByIdAsync(Guid skillId, Guid userId)
    {
        return await _context.UserSkills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);
    }

    public async Task RemoveSkillAsync(DbUserSkill skill)
    {
        _context.UserSkills.Remove(skill);
        await _context.SaveChangesAsync();
    }
}