using ApiBase.Models;
using System.Security.Cryptography;

namespace ApiBase.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<bool> MarkRefreshTokenUsedAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(string userId);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly List<RefreshToken> _refreshTokens = new();
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            JwtId = jwtId,
            UserId = userId,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")),
            Used = false,
            Invalidated = false
        };

        _refreshTokens.Add(refreshToken);
        return await Task.FromResult(refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await Task.FromResult(_refreshTokens.FirstOrDefault(x => x.Token == token));
    }

    public async Task<bool> MarkRefreshTokenUsedAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken == null)
            return false;

        refreshToken.Used = true;
        return true;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken == null)
            return false;

        refreshToken.Invalidated = true;
        return true;
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var userTokens = _refreshTokens.Where(x => x.UserId == userId).ToList();
        foreach (var token in userTokens)
        {
            token.Invalidated = true;
        }
        await Task.CompletedTask;
    }
}
