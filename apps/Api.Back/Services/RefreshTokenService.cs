using StackExchange.Redis;
using System.Security.Cryptography;

namespace Api.Back.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(string identityId, string deviceId);
    Task StoreRefreshTokenAsync(string identityId, string deviceId, string rawToken, DateTime expirationTime);
    Task<bool> ValidateRefreshTokenAsync(string identityId, string deviceId, string token);
    Task RevokeRefreshTokenAsync(string identityId, string deviceId);
    Task RevokeAllUserTokensAsync(string identityId);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IConnectionMultiplexer _redis;
    private const string TokenKeyPrefix = "refresh_token";

    public RefreshTokenService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    /// <summary>
    /// Génère un token sécurisé de 32 bytes (256 bits)
    /// </summary>
    public Task<string> GenerateRefreshTokenAsync(string identityId, string deviceId)
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Task.FromResult(Convert.ToBase64String(randomBytes));
    }

    /// <summary>
    /// Hache le token et le stocke dans Redis avec TTL automatique
    /// </summary>
    public async Task StoreRefreshTokenAsync(string identityId, string deviceId, string rawToken, DateTime expirationTime)
    {
        var db = _redis.GetDatabase();
        var key = $"{TokenKeyPrefix}:{identityId}:{deviceId}";

        var tokenHash = HashToken(rawToken);
        var ttl = expirationTime - DateTime.UtcNow;

        await db.StringSetAsync(key, tokenHash, ttl);
    }

    /// <summary>
    /// Valide le refresh token fourni
    /// </summary>
    public async Task<bool> ValidateRefreshTokenAsync(string identityId, string deviceId, string token)
    {
        var db = _redis.GetDatabase();
        var key = $"{TokenKeyPrefix}:{identityId}:{deviceId}";

        var storedHash = await db.StringGetAsync(key);

        if (!storedHash.HasValue)
            return false;

        var providedHash = HashToken(token);
        return storedHash.ToString() == providedHash;
    }

    /// <summary>
    /// Révoque le refresh token d'un device spécifique
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string identityId, string deviceId)
    {
        var db = _redis.GetDatabase();
        var key = $"{TokenKeyPrefix}:{identityId}:{deviceId}";
        await db.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Révoque tous les refresh tokens de l'utilisateur (logout global)
    /// </summary>
    public async Task RevokeAllUserTokensAsync(string identityId)
    {
        var db = _redis.GetDatabase();
        // !! _redis.GetEndPoints().First() suppose un seul noeud Redis,
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        var keys = server.Keys(pattern: $"{TokenKeyPrefix}:{identityId}:*");

        foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }
    }

    /// <summary>
    /// Hash un token avec SHA256 pour stockage sécurisé
    /// </summary>
    private static string HashToken(string token)
    {
        var hashedBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}