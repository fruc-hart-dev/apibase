using ApiBase.Data;
using ApiBase.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiBase.Services;

public class DbRefreshTokenService : IRefreshTokenService
{
    private readonly ApiDbContext _context;

    public DbRefreshTokenService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            JwtId = jwtId,
            UserId = Guid.Parse(userId),
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Used = false,
            Invalidated = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<bool> MarkRefreshTokenUsedAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FindAsync(token);
        if (refreshToken == null)
            return false;

        refreshToken.Used = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FindAsync(token);
        if (refreshToken == null)
            return false;

        refreshToken.Invalidated = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var userTokens = await _context.RefreshTokens
            .Where(x => x.UserId == Guid.Parse(userId))
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.Invalidated = true;
        }

        await _context.SaveChangesAsync();
    }
}
