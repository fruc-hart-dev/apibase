using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiBase.Models;
using Microsoft.IdentityModel.Tokens;

namespace ApiBase.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
    Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IUserService userService, 
        IConfiguration configuration,
        IRefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _configuration = configuration;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userService.GetUserByNameAsync(request.Username);
        if (user == null)
            return null;
            
        // Verify the password
        if (!PasswordHasher.VerifyPassword(request.Password, user.Password))
            return null;

        // Generate JWT token and refresh token
        var (token, jwtId) = GenerateJwtToken(user);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id.ToString(), jwtId);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            Username = user.Name
        };
    }

    private (string token, string jwtId) GenerateJwtToken(User user)
    {
        var jwtId = Guid.NewGuid().ToString();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), jwtId);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        var savedRefreshToken = await _refreshTokenService.GetRefreshTokenAsync(refreshToken);
        if (savedRefreshToken == null)
            return null;

        // Validate refresh token
        if (savedRefreshToken.ExpiryDate < DateTime.UtcNow ||
            savedRefreshToken.Used ||
            savedRefreshToken.Invalidated)
            return null;

        // Mark current refresh token as used
        await _refreshTokenService.MarkRefreshTokenUsedAsync(refreshToken);

        // Get user and generate new tokens
        var user = await _userService.GetUserByIdAsync(savedRefreshToken.UserId);
        if (user == null)
            return null;

        var (token, jwtId) = GenerateJwtToken(user);
        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id.ToString(), jwtId);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = newRefreshToken.Token,
            Username = user.Name
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        return await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);
    }
}
