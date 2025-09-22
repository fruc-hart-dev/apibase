using ApiBase.Models;

namespace ApiBase.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string jwtId);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<bool> MarkRefreshTokenUsedAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(string userId);
}
