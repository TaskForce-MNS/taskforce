using Api.Back.Data;
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
        return await _context.Identities
            .Include(i => i.Credentials)
            .Include(i => i.Preference)
            .FirstOrDefaultAsync(i => i.Credentials.Any(c => c.PublicKey.SequenceEqual(Convert.FromBase64String(publicKey))));
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
}